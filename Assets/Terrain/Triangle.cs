using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle {

    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

	public Triangle(Vector3 a, Vector3 b, Vector3 c) {
		this.a = a;
		this.b = b;
		this.c = c;
	}
	public Vector3 CalcNormal() {
		
		Vector3 U = b - a;
		Vector3 V = c - b;

		return new Vector3(
            U.y * V.z - U.z * V.y,
            U.z * V.x - U.x * V.z,
            U.x * V.y - U.y * V.x
        );
    }
}