using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboTextHandler : MonoBehaviour
{
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    public TextMeshProUGUI comboTextField;

    public string[] comboTexts;
    public Color[] comboColors;

    public void SetComboText(Vector3 pos)
    {
        comboTextField.transform.position = pos;
        comboTextField.text = comboTexts[Random.Range(0, comboTexts.Length)];
        comboTextField.fontMaterial.SetColor(OutlineColor, comboColors[Random.Range(0, comboColors.Length)]);

        Sequence comboTextSequence = DOTween.Sequence();

        Tween spawnPunchTween = comboTextField.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 0, 0).SetEase(Ease.InQuad);
        Tween moveUpTween = comboTextField.transform.DOMoveY(comboTextField.transform.position.y + 0.25f, 0.5f).SetEase(Ease.Linear);
        Tween fadeOutTween = comboTextField.DOFade(0, 0.5f).SetEase(Ease.Linear);

        comboTextSequence.Append(spawnPunchTween);
        comboTextSequence.Join(moveUpTween);
        comboTextSequence.Append(fadeOutTween);
        comboTextSequence.OnComplete(() =>
        {
            comboTextField.text = "";
            comboTextField.color = Color.white;
        });
    }
}
