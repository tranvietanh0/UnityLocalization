#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace The1Studio.UnityLocalizationTool.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedTmpText : MonoBehaviour
    {
        [SerializeField] private TMP_Text? targetText;
        [SerializeField] private LocalizedString localizedString = new();

        private void Awake()
        {
            this.targetText ??= this.GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            this.localizedString.StringChanged += this.UpdateText;
            this.localizedString.RefreshString();
        }

        private void OnDisable()
        {
            this.localizedString.StringChanged -= this.UpdateText;
        }

        public void SetReference(string table, string key)
        {
            this.localizedString.TableReference = table;
            this.localizedString.TableEntryReference = key;
            this.localizedString.RefreshString();
        }

        public void SetArguments(params object[] arguments)
        {
            this.localizedString.Arguments = arguments;
            this.localizedString.RefreshString();
        }

        private void UpdateText(string value)
        {
            if (this.targetText == null)
            {
                return;
            }

            this.targetText.text = value;
        }
    }
}
