using UnityEngine;
using static Michsky.UI.Reach.HotkeyEvent;

namespace Michsky.UI.Reach
{
    [RequireComponent(typeof(SettingsElement))]
    public class SettingsDescription : MonoBehaviour
    {
        [Header("Resources")]
        public SettingsDescriptionManager manager;
        public SettingsElement element;

        [Header("Content")]
        [SerializeField] private Sprite cover;
        [SerializeField] private string title = "Title";
        [SerializeField][TextArea] private string description = "Description area.";

        [Header("Localization")]
        [SerializeField] private string titleKey;
        [SerializeField] private string descriptionKey;

        void Start()
        {
            if (manager == null && FindObjectsOfType(typeof(SettingsDescriptionManager)).Length > 0) 
            { 
                manager = (SettingsDescriptionManager)FindObjectsOfType(typeof(SettingsDescriptionManager))[0];
            }
            else if (manager == null) { Destroy(this); }

            if (element == null) { element = gameObject.GetComponent<SettingsElement>(); }

            element.onHover.AddListener(delegate
            {
                if (manager == null)
                    return;

                if (manager.localizedObject != null && manager.useLocalization == true && !string.IsNullOrEmpty(titleKey) && !string.IsNullOrEmpty(descriptionKey)) 
                {
                    manager.UpdateUI(manager.localizedObject.GetKeyOutput(titleKey), manager.localizedObject.GetKeyOutput(descriptionKey), cover); 
                }
                else { manager.UpdateUI(title, description, cover); }
            });

            element.onLeave.AddListener(delegate
            {
                if (manager == null)
                    return;

                manager.SetDefault();
            });
        }
    }
}