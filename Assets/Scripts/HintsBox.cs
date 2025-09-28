using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintsBox : MonoBehaviour
{
    public GameObject MainHintsBox;
    public Image KeyImage;
    public Text HintsText;

    public List<Sprite> Sprites;
    public List<string> HintsStrList;

    public int frameSize = 100;
    public float animTime = 0.5f;

    private RectTransform KeySize;
    private Vector2 StartKeySize;
    private void Start()
    {
       
    }

    class HintObj
    {
        public Sprite KeySprite { get; set; }
        public string HintTest { get; set; }
    }
    
    private HintObj selectedHintObj;
    private CanvasGroup _canvasGroup;

    private void HidMainHintsBox()
    {
        MainHintsBox.SetActive(false);
    }
    
    private void ShowMainHintsBox()
    {
        MainHintsBox.SetActive(true);
    }

    private IEnumerator onAnim()
    {
        for (float i = 0; i < frameSize; i++)
        {
            _canvasGroup.alpha = (i / frameSize) ;
            
            yield return new WaitForSeconds((float)animTime / frameSize);
        }
    }

    private IEnumerator offAnim()
    {
        for (float i = 0; i < frameSize; i++)
        {
            // 1 - albedo 100%
            
            _canvasGroup.alpha = 1 - (i / frameSize) ;
            
            yield return new WaitForSeconds((float)animTime / frameSize);
        }

        HidMainHintsBox();
    }

    private IEnumerator WaitHid(HintObj hintObj)
    {
        yield return new WaitForSeconds(5);
        if (selectedHintObj==hintObj)
        {
            StartCoroutine(offAnim());
        }
        
    }

    void SetImageSize(int hintId)
    {
        if (Sprites[hintId].bounds.size.x != Sprites[hintId].bounds.size.y)
        {
            var sizeDelta = KeySize.sizeDelta;
            
            sizeDelta = new Vector2(StartKeySize.y, StartKeySize.y / 3.3f);
            
            KeySize.sizeDelta = sizeDelta;
        }
        else
        {
            var sizeDelta = KeySize.sizeDelta;
            
            sizeDelta = new Vector2(StartKeySize.x, StartKeySize.x );
            
            KeySize.sizeDelta = sizeDelta;
        }
    }

    
    public void SetHintData(int hintId)
    {

        if (StartKeySize!=null)
        {
            StartKeySize = new Vector2(70, 70);
        }
        
        _canvasGroup = MainHintsBox.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        
        
        
        KeySize = KeyImage.GetComponent<RectTransform> ();
        
        
        
        
        KeyImage.sprite = Sprites[hintId];
        HintsText.text = HintsStrList[hintId];


        SetImageSize(hintId);
        
        
        
        
        
        ShowMainHintsBox();
        
        
        StartCoroutine(onAnim());

        
        selectedHintObj = new HintObj() { KeySprite = Sprites[hintId], HintTest = HintsStrList[hintId]};
        
        
        StartCoroutine(WaitHid(selectedHintObj));
    }

}
