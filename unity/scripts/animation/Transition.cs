using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.UIElements;
using System.ComponentModel;

public class Transition : MonoBehaviour
{
    public TextMeshProUGUI logo, logoShadow, text;
    public UnityEngine.UI.Image plane;
    public UnityEngine.UI.Image rainPanel;
    public Camera camer;
    [SerializeField]
    ParticleSystem rainParticles;
    public Light svet;
    public Light[] polesLights;
    public UnityEngine.UI.Image img;
    [SerializeField]
    private RectTransform arrowTransform;
    private Vector2 oldPos;
    public Transform poleY;
    private bool showed = false;

    bool wasIsLight = false;
    bool wasIsRain = false;

    bool lightNeedUpdate = true;
    bool rainNeedUpdate = true;


    private void Start()
    {
        DOTween.Sequence()
            .AppendCallback(ShowLogo)
            .AppendInterval(1f)
            .AppendCallback(MoveLogo)
            .AppendInterval(1f)
        .Append(plane.DOFade(0f, 1.25f).SetEase(Ease.InOutSine));
        rainParticles.Stop();
    }
    public void ShowLogo()
    {
        logo.DOFade(1f, 1f).SetEase(Ease.InOutSine);
        logoShadow.DOFade(0.5f, 1f).SetEase(Ease.InOutSine);
    }
    public void MoveLogo()
    {
        logo.rectTransform.DOAnchorPos(new Vector2(0, 0), 1f).SetEase(Ease.InOutSine);
        logo.rectTransform.DOScale(0.2f, 1f).SetEase(Ease.InOutSine);
        logoShadow.rectTransform.DOAnchorPos(new Vector2(0, 0), 1f).SetEase(Ease.InOutSine);
        logoShadow.rectTransform.DOScale(0.2f, 1f).SetEase(Ease.InOutSine);
    }
    public void ShowCanvas()
    {
        if (showed == false)
        {
            oldPos = img.rectTransform.anchoredPosition;
            img.rectTransform.DOAnchorPos(Vector2.zero, 1f);
            arrowTransform.DOLocalRotate(new Vector3(0, 0, 270), 1f);

            poleY.DOLocalMove(new Vector3(-45f, -15f, -100f), 1f);

        }
        else
        {
            img.rectTransform.DOAnchorPos(oldPos, 1f);
            arrowTransform.DOLocalRotate(new Vector3(0, 0, 90), 1f);

            poleY.DOLocalMove(new Vector3(0f, -23f, 0f), 1f);

        }
        showed = !showed;
    }
    private void Update()
    {
        if (ServerScript.isLight != wasIsLight) lightNeedUpdate = true;
        if (ServerScript.isRain != wasIsRain) rainNeedUpdate = true;

        wasIsLight = ServerScript.isLight;
        wasIsRain = ServerScript.isRain;

        if (lightNeedUpdate)
        {
            if (!ServerScript.isLight) svet.transform.DORotate(new Vector3(205f, svet.transform.rotation.y, svet.transform.rotation.z), 0.66f);
            else svet.transform.DORotate(new Vector3(90f, svet.transform.rotation.y, svet.transform.rotation.z), 2f);
            lightNeedUpdate = false;
        }
        

        if (rainNeedUpdate)
        {
            if (!ServerScript.isRain)
            {
                rainPanel.DOFade(0f, 1f);
                for (int i = 0; i < polesLights.Length; i++)
                {
                    if (i != 7) polesLights[i].DOColor(Color.white, 1f);
                    else polesLights[i].DOColor(Color.red, 1f);
                }
                rainParticles.gameObject.SetActive(true);
                rainParticles.Stop();
            }
            else
            {
                rainPanel.DOFade(0.7f, 1f);
                for (int i = 0; i < polesLights.Length; i++)
                {
                    if(i != 7) polesLights[i].DOColor(Color.yellow, 1f);
                    else polesLights[i].DOColor(Color.red, 1f);
                }
                rainParticles.Play();
            }
        }
        
    }
}
