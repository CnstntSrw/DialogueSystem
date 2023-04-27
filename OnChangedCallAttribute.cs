using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomPropertyAttributes
{
#if UNITY_EDITOR
    public class OnChangedCallAttribute : PropertyAttribute
    {
        public string methodName;
        public OnChangedCallAttribute(string methodNameNoArguments)
        {
            methodName = methodNameNoArguments;
        }
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[")];
                    var index = System.Convert.ToInt32(element[element.IndexOf("[")..].Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
            }
            return obj;
        }
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }
        private static object GetValue_Imp(object source, string name, int index)
        {
            if (GetValue_Imp(source, name) is not System.Collections.IEnumerable enumerable) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }


    [CustomPropertyDrawer(typeof(OnChangedCallAttribute))]
    public class OnChangedCallAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                OnChangedCallAttribute at = attribute as OnChangedCallAttribute;
                var type = property.serializedObject.targetObject.GetType();
                object targetObject;
                if (type == typeof(DialogueSystem.Dialogue))
                {
                    targetObject = OnChangedCallAttribute.GetTargetObjectOfProperty(property);
                }
                else
                {
                    targetObject = property.serializedObject.targetObject;
                }
                MethodInfo method = targetObject.GetType().GetMethods().Where(m => m.Name == at.methodName).First();
                property.serializedObject.ApplyModifiedProperties();
                if (method != null && method.GetParameters().Count() == 0)// Only instantiate methods with 0 parameters
                    method.Invoke(targetObject, null);
            }
        }
    }

#endif 
}