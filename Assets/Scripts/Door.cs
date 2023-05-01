using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject alert;
    [SerializeField] TMP_Text counter;
    public int deliveriesNeeded;
    public void AddDelivery() {
        deliveriesNeeded++;

        alert.SetActive(true);

        if(deliveriesNeeded > 0) counter.text = "x" + deliveriesNeeded;
        else counter.text = "";
    }
    public void SubDelivery() {
        deliveriesNeeded--;
        GameData.Instance.deliveredPackages++;

        if(deliveriesNeeded > 0) counter.text = "x" + deliveriesNeeded;
        else {
            alert.SetActive(false);
            counter.text = "";
            GameData.Instance.CompleteDoor(this);
        }

        if(deliveriesNeeded < 0) deliveriesNeeded = 0;
    }
}
