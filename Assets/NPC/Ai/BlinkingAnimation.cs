using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BlinkingAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Animator _animator;

    private float RandomNumber;

    private float NeededNumber;

    void Start()
    {
        RandomNumberMethod();
    }

    // Update is called once per frame
    void Update()
    {
        RandomNumber = RandomNumber - Time.deltaTime;
    }

    private void RandomNumberMethod()
    {

        RandomNumber = Random.Range(1, 11);

        while ( RandomNumber > 0)
        {
            _animator.SetBool("isBlinking", false);
            Debug.Log(NeededNumber);


        }      
        Debug.Log(RandomNumber);
        RandomNumber = Random.Range(1, 11);
        _animator.SetBool("isBlinking", true);


    }

}
