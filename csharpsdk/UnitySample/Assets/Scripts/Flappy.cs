using System;
using UnityEngine;

public class Flappy : MonoBehaviour
{
    [Range(30f, 60f)]
    [SerializeField] public float strength;
    [Range(1f, 1.5f)]
    [SerializeField] public float gravity;
    [Range(0.7f, 1.3f)]
    [SerializeField] public float size;
    [Range(1f, 1.5f)]
    [SerializeField] public float speed;
    [Range(0, 0.3f)]
    [SerializeField] public float luck;

    private Rigidbody2D _rigidBody;
    private Animator _animator;
    public Rigidbody2D Body => _rigidBody;

    private void Awake()
    {
        _rigidBody = GetComponentInChildren<Rigidbody2D>();
        _rigidBody.simulated = false;

        _animator = GetComponentInChildren<Animator>();

        UpdateStats();
    }

    public void Complete()
    {
        _rigidBody.simulated = false;
        _animator.SetBool("IsCompleted", true);
    }

    public void UpdateStats()
    {
        _rigidBody.gravityScale = gravity;
        gameObject.transform.localScale = new Vector3(size, size, size);
    }

    public void ResetValues()
    {
        _rigidBody.simulated = false;
        _rigidBody.transform.localPosition = Vector3.zero;
        _rigidBody.velocity = Vector2.zero;

        _animator.SetBool("IsCompleted", false);
        _animator.Play("Fly");
    }
}
