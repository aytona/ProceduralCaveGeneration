using UnityEngine;
using System.Collections;

public class Player2D: MonoBehaviour {

    Rigidbody2D rigidBody;
    Vector2 velocity;

	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * 10;
	}

    void FixedUpdate()
    {
        rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
    }
}
