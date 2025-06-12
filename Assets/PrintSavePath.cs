using TMPro;
using UnityEngine;

public class PrintSavePath : MonoBehaviour
{
   [SerializeField] private TMP_Text text;

   void Start()
   {
      text.text = Application.persistentDataPath;
   }

}
