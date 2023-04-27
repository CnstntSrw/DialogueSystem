using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Spine.Unity;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


namespace DialogueSystem
{
    public class CharacterPrefab : SerializedMonoBehaviour
    {
        [SerializeField] private string _characterName;

        public string CharacterName
        {
            get => _characterName;
            private set => _characterName = value;
        }

        [SerializeField] private VideoPlayer _lipsingPlayer;

        public VideoPlayer LipsingPlayer
        {
            get => _lipsingPlayer;
            private set => _lipsingPlayer = value;
        }

        [NonSerialized, OdinSerialize, HideInInspector]
        public Dictionary<string, LipsingData> LipsingOutputs = new();

        [SerializeField] public List<LipsingData> LipsingOutputsList = new();

        [SerializeField] private GameObject _getEvent;

        public GameObject GetEvent
        {
            get => _getEvent;
            private set => _getEvent = value;
        }

        [SerializeField] private GameObject _useEvent;

        public GameObject UseEvent
        {
            get => _useEvent;
            private set => _useEvent = value;
        }

        public Transform NoteSlot;

        [SerializeField] public List<SkeletonGraphic> _skeletonAnimators = new();

        private List<DialogueAnimationData> _animationNamesToAnimators;
        private LipsingData _concreteLipsing;
        private bool _playAfterLipsing;

        public void SetAnimations(List<DialogueAnimationData> animationNamesToAnimators, string lipsingKey,
            bool loopFirstAnimation, Action onFirstAnimationEnd)
        {
            if (_skeletonAnimators == null || _skeletonAnimators.Count == 0)
            {
                Debug.LogWarning("_skeletonAnimators of " + this.name + "is empty! Fill it to play animations!");
                return;
            }

            if (animationNamesToAnimators == null || animationNamesToAnimators.Count == 0)
            {
                Debug.LogWarning("animationNamesToAnimators of dialogue SO is empty! Fill it to play animations!");
                return;
            }

            _animationNamesToAnimators = animationNamesToAnimators;
            _playAfterLipsing = false;
            foreach (var lipsingOutput in LipsingOutputsList)
            {
                if (lipsingOutput.Output)
                    lipsingOutput.Output.gameObject.SetActive(false);
            }

            foreach (var data in _animationNamesToAnimators)
            {
                var animator = _skeletonAnimators.FirstOrDefault(a => a.name == data.Animator.name);
                animator.gameObject.SetActive(true);


                if (_animationNamesToAnimators.IndexOf(data) == 0)
                {
                    foreach (var anim in _skeletonAnimators)
                    {
                        if (animator != anim)
                        {
                            anim.gameObject.SetActive(false);
                        }
                    }
                }

                if (animator != null)
                {
                    if (_animationNamesToAnimators.First() == data)
                    {
                        _playAfterLipsing = data.WaitForLipsyncEnd;
                        var trackEntry = animator.AnimationState.SetAnimation(0, data.AnimationName,
                            (_animationNamesToAnimators.Count < 2 || lipsingKey != "None") &&
                            loopFirstAnimation);
                        trackEntry.Complete += entry => { onFirstAnimationEnd?.Invoke(); };
                        // trackEntry.MixDuration = 2f;
                    }
                    else if (!_playAfterLipsing)
                    {
                        var trackEntry = animator.AnimationState.AddAnimation(0, data.AnimationName,
                            _animationNamesToAnimators.Last() == data, 0);
                        // trackEntry.MixDuration = 2f;
                    }
                }
                else
                {
                    Debug.LogWarning("Animator " + data.Animator.name +
                                     " from dialogue SO not found in _skeletonAnimators of " + this.name);
                }
            }

            if (lipsingKey != "None")
            {
                PlayLipsing(lipsingKey);
            }
        }

#if UNITY_EDITOR
        [Button("Migrate lipsyncs data from dictionary to list"),
         HideIf("@LipsingOutputs == null || LipsingOutputs.Count == 0")]
        private void Migrate()
        {
            LipsingOutputsList.Clear();
            foreach (var oldData in LipsingOutputs)
            {
                var newData = new LipsingData();
                newData.Name = oldData.Key;
                newData.Output = oldData.Value.Output;
                newData.HeadRenderer = oldData.Value.HeadRenderer;
                newData.Clip = oldData.Value.Clip;
                LipsingOutputsList.Add(newData);
            }

            LipsingOutputs.Clear();
            LipsingOutputs = null;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }
#endif


        private void PlayLipsing(string lipsingKey)
        {
            if (LipsingOutputsList != null)
            {
                _concreteLipsing = LipsingOutputsList.FirstOrDefault(ld => ld.Name == lipsingKey);

                foreach (var lipsingOutput in LipsingOutputsList)
                {
                    if (lipsingOutput.Output != _concreteLipsing?.Output)
                    {
                        lipsingOutput.Output.gameObject.SetActive(false);
                        if (lipsingOutput.HeadRenderer)
                            lipsingOutput.HeadRenderer.gameObject.SetActive(true);
                    }
                }

                // ClearOutRenderTexture(_lipsingPlayer.targetTexture);
                if (_concreteLipsing != null)
                {
                    _lipsingPlayer.clip = _concreteLipsing.Clip;
                    _lipsingPlayer.Play();
                    _lipsingPlayer.started += source =>
                    {
                        _concreteLipsing.Output.gameObject.SetActive(true);
                        if (_concreteLipsing.HeadRenderer)
                            _concreteLipsing.HeadRenderer.SetActive(false);
                    };

                    _lipsingPlayer.loopPointReached += LipsingPlayerOnloopPointReached;
                }
            }
        }

        private void LipsingPlayerOnloopPointReached(VideoPlayer source)
        {
            _lipsingPlayer.loopPointReached -= LipsingPlayerOnloopPointReached;
            if (_concreteLipsing.HeadRenderer)
                _concreteLipsing.HeadRenderer.SetActive(true);
            _concreteLipsing.Output.gameObject.SetActive(false);

            if (_playAfterLipsing && _animationNamesToAnimators != null)
            {
                var animator = _concreteLipsing.Output.gameObject.GetComponentInParent<SkeletonGraphic>();
                animator.AnimationState.Tracks.Items[0].Loop = false;
                foreach (var data in _animationNamesToAnimators)
                {
                    if (data != _animationNamesToAnimators.First())
                    {
                        var trackEntry = animator.AnimationState.AddAnimation(0, data.AnimationName,
                            !data.IsGetAnimation &&
                            _animationNamesToAnimators.Last() == data, 0);
                    }
                }
            }
        }

        public void ClearOutRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        [Serializable]
        public class LipsingData
        {
            public string Name;
            public RawImage Output;
            public VideoClip Clip;
            public GameObject HeadRenderer;
        }
    }
}