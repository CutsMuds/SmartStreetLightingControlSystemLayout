using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TextChange : MonoBehaviour
{
    public Button leftArrow, rightArrow;
    private int slide = 0;
    public TextMeshProUGUI text;
    bool updateText = false;
    string newText = "";

    const string firstPageText = "\"SMART система керування вуличним " +
        "освітленням\":\n-Відстежує стан світлових точок\n-Змінює " +
        "теплову гамму при наявності опадів або туману\n-Виявляє наяність " +
        "об'єктів для яких потрібно підсвічувати маршрут\n-Економить більше " +
        "50% від споживання електроенергії";
    const string secondPageText = "Цей додаток відображає макет та дозволяє слідкувати за станом " +
        "світлових точок та зовнішніми умовами у реальному часі. Тут можна " +
        "побачити всі проблемні опори, які підсвічуються червоним світлом";
    const string thirdPageText = "Технічна частина:\nмакет скадається з: головної плати керування(esp32)"+
        ", плати, яка стежить за опадами та освітленністю(esp12), і планшету, який є сервером" +
        " та все зв'язує. Живиться все за допомогою 3 акумуляторів";

    public void SwipeChange(bool itIsLeftButton )
    {
        if (slide == 0)    // С 1 на 2
        {
            slide++;
           
            Animation(1);
            newText = secondPageText;
            leftArrow.gameObject.SetActive(true);
            return;   
        }
        if (slide == 1)
        {
            if (itIsLeftButton == false)  // С 2 на 3
            {
                slide++;
                Animation(1);
                newText = thirdPageText;
                rightArrow.gameObject.SetActive(false);
                return;
            }
            if (itIsLeftButton == true) // С 2 на 1
            {
                slide--;
                Animation(-1);
                newText = firstPageText;
                leftArrow.gameObject.SetActive(false);
                return;

            }
        }
        if (slide == 2) // С 3 на 2
        {
            slide--;
            Animation(-1);
            newText = secondPageText;
            rightArrow.gameObject.SetActive(true);
            return;
        }
    }
    Stopwatch animationTimer = new Stopwatch();
    public void Update()
    {
        if (updateText)
        {
            animationTimer.Restart();
            updateText = false;
        }
        if(animationTimer.ElapsedMilliseconds >= 500)
        {
            text.text = newText;
        }
    }
    public void Start()
    {
        text.text = firstPageText;
    }
    public void Animation(int i)
    {
        DOTween.Sequence()
            .Append(text.transform.DOLocalMoveY(-340f * i, 0.5f).SetEase(Ease.InQuint))
            .Append(text.transform.DOLocalMoveY(404f * i, 0f))
             .Append(text.rectTransform.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutQuint));

        DOTween.Sequence()
           .Append(text.DOFade(0f, 0.5f).SetEase(Ease.OutQuint))
           .Append(text.DOFade(1f, 0.5f).SetEase(Ease.InQuint));

        updateText = true;
    }
}
