 //CONTROLLO MACCHINA
 
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LPPV_CarController : MonoBehaviour {


	public enum WheelType
	{
		FrontLeft, FrontRight, RearLeft, RearRight
	}
	public enum SpeedUnit
	{
		Imperial, Metric
	}
	[Serializable]
	public class Wheel
	{
		public WheelCollider collider;
		public GameObject mesh;
		public WheelType wheelType;
	}

	[SerializeField] public Wheel[] wheels = new Wheel[4];
	[SerializeField] private float maxTorque = 2000f, maxBrakeTorque = 500f, maxSteerAngle = 30f; 
	[SerializeField] private float downForce = 100f; 
	[SerializeField] private SpeedUnit speedUnit;
	[SerializeField] public float topSpeed = 140;
	[SerializeField] private Transform centerOfMass;
	[SerializeField] private Text speedText;



	[HideInInspector] public bool Accelerating = false, Deccelerating = false, HandBrake = false;
	private Rigidbody _rgbd;
	public float CurrentSpeed{
		get 
		{  
			float speed = _rgbd.velocity.magnitude;
			if (speedUnit == SpeedUnit.Imperial)
				speed *= 2.23693629f;
			else
				speed *= 3.6f; 
			return speed;
		}
	}

	private void VisualizeWheel(Wheel wheel)
	{
		//Copia la posizione del collider nel mesh
		if (wheel.mesh == null || wheel.collider == null)
			return;

		Vector3 position;
		Quaternion rotation;
		wheel.collider.GetWorldPose(out position, out rotation);

		wheel.mesh.transform.position = position;
		wheel.mesh.transform.rotation = rotation;
	}

	
	//AZIONI INIZIALI

	private void Start () 
	{
		_rgbd = GetComponent<Rigidbody> ();
		if (centerOfMass != null && _rgbd != null)
			_rgbd.centerOfMass = centerOfMass.localPosition;
		for (int i = 0; i < wheels.Length; ++i)
			VisualizeWheel (wheels [i]);
	}

	//FUNZIONE PRINCIPALE PER IL MOVIMENTO, utilizzata anche con riferimento dall'agente


	public void Move(float motorInput, float steerInput, bool handBrake)
	{
		HandBrake = handBrake;
		motorInput = Mathf.Clamp(motorInput, -1f, 1f);

		// GESTIONE INPUT
		if (motorInput > 0f)
		{
			// Accelerate vehicle!
			Accelerating = true;
			Deccelerating = false;
		}
		else if (motorInput < 0f)
		{
			// Brake Vehicle!
			Accelerating = false;
			Deccelerating = true;
		}
		else
		{
			// No acceleration or braking - Gradual braking
			Accelerating = false;
			Deccelerating = false;

		}

		steerInput = Mathf.Clamp (steerInput, -1f, 1f);
		float steer = steerInput * maxSteerAngle;


		for (int i = 0; i < wheels.Length; ++i) 
		{
			if (wheels [i].collider == null)
				break;
			if (wheels [i].wheelType == WheelType.FrontLeft || wheels [i].wheelType == WheelType.FrontRight) 
			{
				wheels [i].collider.steerAngle = steer;

			}
			if (!handBrake) 
			{	
				//MANTIENI VELOCITA'
				wheels [i].collider.brakeTorque = 0f;
				//wheels [i].collider.motorTorque = 0f;

				if (Accelerating)
					wheels [i].collider.motorTorque = motorInput * maxTorque / 4f;
				else if (Deccelerating)
					wheels [i].collider.motorTorque = motorInput * maxBrakeTorque / 20f;
				else{
					wheels [i].collider.motorTorque = 0f;
					//wheels [i].collider.brakeTorque = maxBrakeTorque * 50f;
				}
			} else 
			{	
				Accelerating = false;
				wheels [i].collider.brakeTorque = maxBrakeTorque * 50f;
				wheels [i].collider.motorTorque = 0f;
				
			}
			if(wheels[i].mesh != null)
				VisualizeWheel (wheels [i]);
		}
		
		StickToTheGround ();
		ManageSpeed ();
	}

	//DOWNFORCE per tenere il veicolo sulla strada

	private void StickToTheGround()
	{
		//if (wheels [0].collider == null)
		//	return;
		wheels [0].collider.attachedRigidbody.AddForce (-transform.up * downForce * wheels [0].collider.attachedRigidbody.velocity.magnitude);
	}


	//LIMITATORE VELOCITA'
	private void ManageSpeed()
	{
		float speed = _rgbd.velocity.magnitude;
		switch (speedUnit)
		{
		case SpeedUnit.Imperial:
			speed *= 2.23693629f;
			if (speed > topSpeed)
				_rgbd.velocity = (topSpeed/2.23693629f) * _rgbd.velocity.normalized;
			break;

		case SpeedUnit.Metric:
			speed *= 3.6f;
			if (speed > topSpeed)
				_rgbd.velocity = (topSpeed/3.6f) * _rgbd.velocity.normalized;
			break;
		}
	}

	private string imp = " MPH", met = " KPH";
	private void Update()
	{
		if (speedText != null)
		{
			if(speedUnit == SpeedUnit.Imperial)
				speedText.text = ((int)CurrentSpeed).ToString () + imp;
			else
				speedText.text = ((int)CurrentSpeed).ToString () + met;
		}
	}

	//INPUTS (controllati da heuristic ora)
	/*private void FixedUpdate()
	{
		float motor = 0f, steering = 0f;
		bool handBrakeInput = false;
		#if UNITY_STANDALONE || UNITY_WEBGL
			motor = Input.GetAxis("Vertical");
			steering = Input.GetAxis("Horizontal");
			handBrakeInput = Input.GetButton ("Jump");
		#endif
		Move (motor, steering, handBrakeInput);
	}
	*/

}
