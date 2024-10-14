using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleJumpScares : MonoBehaviour
{
    public GameObject notCaughtScares;
    public GameObject caughtScares;
    public GameObject torchDepleteScares;

    public void NotCaughtScare()
    {
        int jumpScareNo = new System.Random().Next(0, NotCaughtScares.noOfScares);
        notCaughtScares.GetComponent<NotCaughtScares>().enabled = true; // enable scare script now
        notCaughtScares.GetComponent<NotCaughtScares>().ChooseJumpScare(jumpScareNo);
    }

    public void CaughtScare()
    {
        int jumpScareNo = new System.Random().Next(0, CaughtScares.noOfScares);
        caughtScares.GetComponent<CaughtScares>().enabled = true; // enable scare script now
        caughtScares.GetComponent<CaughtScares>().ChooseJumpScare(jumpScareNo);
    }

    public void TorchDepleteScare()
    {
        int jumpScareNo = new System.Random().Next(0, TorchDepleteScares.noOfScares);
        torchDepleteScares.GetComponent<TorchDepleteScares>().enabled = true; // enable scare script now
        torchDepleteScares.GetComponent<TorchDepleteScares>().ChooseJumpScare(jumpScareNo);
    }
}
