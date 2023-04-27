#region

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class DialogueBigNoteControlelr : BigNoteController
{
    public event Action OnCLose;

    public override void CloseNote()
    {
        if (_isInTransition == false)
        {
            if (LockController.Instance)
            {
                LockController.Instance.Lock(1);
            }

            _isInTransition = true;
            if (PlayerInfo.Instance)
                PlayerInfo.Instance.ActiveNote = null;

            gameObject.transform.parent.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            _destination = _smallNote.transform.position;
            _noteTransform = gameObject.GetComponent<RectTransform>();
            _coordinateStep = new Vector3(_destination.x * 3, _destination.y * 3, 0);
            _scaleStep = new Vector3(3, 3, 0);

            StartCoroutine(CloseNoteCoroutine());
        }
    }

    protected override IEnumerator CloseNoteCoroutine()
    {
        _progress = 1;
        AudioSystem.Instance.PlayEffectSound(CommonSounds.paperzoom_out);
        while (_progress > 0)
        {
            _noteTransform.localScale -= _scaleStep * Time.unscaledDeltaTime;
            _noteTransform.transform.localPosition += _coordinateStep * Time.unscaledDeltaTime;

            var smallNoteColor = GetColor(_smallNote);
            smallNoteColor.a += 3 * Time.unscaledDeltaTime;
            SetColor(_smallNote, smallNoteColor);

            _progress -= 3 * Time.unscaledDeltaTime;
            yield return null;
        }

        _noteTransform.localScale = new Vector3(1, 1, 1);
        _noteTransform.transform.localPosition = _destination;
        SetColor(_smallNote, new Color(1, 1, 1, 1));
        _backscreen.SetActive(false);
        _isInTransition = false;
        if (LockController.Instance)
        {
            LockController.Instance.Lock(0);
        }

        Destroy(_isInZZ == true ? gameObject.transform.parent.gameObject : gameObject);
        OnCLose?.Invoke();
    }
}