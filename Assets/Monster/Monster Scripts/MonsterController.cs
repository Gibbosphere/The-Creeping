using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("c")){
            animator.SetBool("crawl", true);
            Debug.Log("bsees");
        }
        if (Input.GetKey("i")){
            animator.SetBool("crawl", false);
            animator.SetBool("run", false);
        }
        if (Input.GetKey("r")){
            animator.SetBool("run", true);
        }
    }
}
