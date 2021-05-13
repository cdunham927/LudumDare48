using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public ScriptableFloat maxHp;
    public ScriptableFloat hp;
    public ScriptableFloat maxStamina;
    public ScriptableFloat stamina;

    public Image hpImage;
    public Image stamImage;

    public float lerpSpd = 7f;

    private void Update()
    {
        hpImage.fillAmount = Mathf.Lerp(hpImage.fillAmount, hp.val / maxHp.val, lerpSpd * Time.deltaTime);
        stamImage.fillAmount = Mathf.Lerp(stamImage.fillAmount, stamina.val / maxStamina.val, lerpSpd * Time.deltaTime);
    }
}
