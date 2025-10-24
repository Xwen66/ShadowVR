using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// XRSocketInteractor 的扩展方法，提供手动插入功能
    /// </summary>
    public static class XRSocketInteractorExtensions
    {
        /// <summary>
        /// 强制将指定的可交互对象插入到插槽中
        /// </summary>
        /// <param name="socket">目标插槽交互器</param>
        /// <param name="interactable">要插入的可交互对象</param>
        /// <param name="snapToSocket">是否将对象吸附到插槽位置</param>
        /// <returns>操作是否成功</returns>
        public static bool ForceInsert(this XRSocketInteractor socket, IXRSelectInteractable interactable, bool snapToSocket = true)
        {
            if (socket == null || interactable == null)
            {
                Debug.LogWarning("Socket 或 Interactable 不能为 null");
                return false;
            }

            // 获取交互管理器
            var interactionManager = socket.interactionManager;
            if (interactionManager == null)
            {
                Debug.LogWarning("找不到 XRInteractionManager");
                return false;
            }

            // 如果对象已经是当前选择，直接返回成功
            if (socket.IsSelecting(interactable))
                return true;

            // 如果插槽已经有选择，先取消选择
            if (socket.hasSelection)
            {
                interactionManager.SelectExit(socket, socket.firstInteractableSelected);
            }

            // 如果对象已被其他交互器选择，先取消选择
            if (interactable.isSelected)
            {
                foreach (var selectingInteractor in interactable.interactorsSelecting)
                {
                    interactionManager.SelectExit(selectingInteractor, interactable);
                }
            }

            // 如果需要吸附到插槽位置，先设置对象位置
            if (snapToSocket && interactable is Component interactableComponent)
            {
                var socketAttachTransform = socket.GetAttachTransform(interactable);
                var interactableAttachTransform = interactable.GetAttachTransform(socket);
                
                // 计算正确的相对偏移并应用
                // 将交互对象的附着点对齐到插槽的附着点
                var offset = interactableAttachTransform.position - interactableComponent.transform.position;
                interactableComponent.transform.position = socketAttachTransform.position - offset;
                
                // 计算旋转差异并应用
                var rotationDifference = socketAttachTransform.rotation * Quaternion.Inverse(interactableAttachTransform.rotation);
                interactableComponent.transform.rotation = rotationDifference * interactableComponent.transform.rotation;
            }

            // 强制选择对象
            interactionManager.SelectEnter(socket, interactable);
            
            // 返回选择是否成功
            return socket.IsSelecting(interactable);
        }

        /// <summary>
        /// 强制将指定的 GameObject 插入到插槽中
        /// </summary>
        /// <param name="socket">目标插槽交互器</param>
        /// <param name="targetObject">要插入的游戏对象</param>
        /// <param name="snapToSocket">是否将对象吸附到插槽位置</param>
        /// <returns>操作是否成功</returns>
        public static bool ForceInsert(this XRSocketInteractor socket, GameObject targetObject, bool snapToSocket = true)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("目标对象不能为 null");
                return false;
            }

            var interactable = targetObject.GetComponent<IXRSelectInteractable>();
            if (interactable == null)
            {
                Debug.LogWarning($"对象 {targetObject.name} 没有实现 IXRSelectInteractable 接口");
                return false;
            }

            return socket.ForceInsert(interactable, snapToSocket);
        }
    }
}