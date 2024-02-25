using System;
using System.Linq;
using UnityEngine;

namespace rStarUtility
{
    /// <summary>
    /// Inspired from: https://forum.unity.com/threads/getcomponentinparent-gets-component-on-itself-instead.1410900/#post-9342449
    /// </summary>
    public static class ComponentExtensions
    {
        private static T GetComponentInParent<T>(this Component component , Self self , Inactive inactive) where T : Component =>
                self switch
                {
                    Self.Include => component.GetComponentInParent<T>(inactive == Inactive.Include) ,
                    _ when component.transform.parent is Transform parent && parent != null => parent.GetComponentInParent<T>(
                            inactive == Inactive.Include) ,
                    _ => null
                };

        public static T GetComponentInParent<T>(this Component component , Self self) where T : Component =>
                component.GetComponentInParent<T>(
                        self , component.gameObject.activeInHierarchy ? Inactive.Exclude : Inactive.Include);

        public static T GetComponentInChildren<T>(this Component component , Self self , Inactive inactive) where T : Component =>
                self switch
                {
                    Self.Include => component.GetComponentInChildren<T>(inactive == Inactive.Include) ,
                    _ when component.transform.childCount != 0 => component.GetComponentsInChildren<T>(inactive == Inactive.Include)
                                                                           .FirstOrDefault(t => t.gameObject != component.gameObject) ,
                    _ => null
                };

        public static T GetComponentInChildren<T>(this Component component , Self self) where T : Component =>
                component.GetComponentInChildren<T>(
                        self , component.gameObject.activeInHierarchy ? Inactive.Exclude : Inactive.Include);
    }

    public enum Self
    {
        /// <summary>
        /// 不包含自己
        /// </summary>
        Exclude ,

        /// <summary>
        /// 包含自己
        /// </summary>
        Include
    }

    public enum Inactive
    {
        /// <summary>
        /// 不包含隱藏
        /// </summary>
        Exclude ,

        /// <summary>
        /// 包含隱藏
        /// </summary>
        Include
    }
}