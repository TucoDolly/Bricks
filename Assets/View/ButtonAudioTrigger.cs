using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Garante que botões sem o componente NeonButtonFx também tenham efeitos sonoros
/// ao passar o mouse e ao clicar.
/// </summary>
[DisallowMultipleComponent]
public class ButtonAudioTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Selectable _selectable;

    private void Awake()
    {
        TryGetComponent(out _selectable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GetComponent<NeonButtonFx>() != null) return; // NeonButtonFx já cuida
        if (_selectable == null || _selectable.IsInteractable())
        {
            AudioController.Instance?.PlayButtonHover();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GetComponent<NeonButtonFx>() != null) return; // NeonButtonFx já cuida
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left) return;
        if (_selectable == null || _selectable.IsInteractable())
        {
            AudioController.Instance?.PlayButtonClick();
        }
    }
}
