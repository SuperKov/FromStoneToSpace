using UnityEngine;

public interface IInteractable
{
    // Избавляемся от приведения типов в цикле поиска
    Transform Transform { get; }

    string GetInteractionID();
    bool Interact(PlayerInteractor interactor);

    // Вызывается контроллером, когда предмет становится/перестает быть ближайшим
    void SetHighlight(bool isHighlighted);
}