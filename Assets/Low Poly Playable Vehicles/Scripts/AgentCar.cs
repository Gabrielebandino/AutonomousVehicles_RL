//AGENTE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentCar : Agent
{
    private RoadGen RoadGen;
    private LPPV_CarController LPPV_CarController;    //fa riferimento allo script per controllare la macchina
    private int counter = 0; //counter per index checkpoint
    private float directionAngle = 0;
    private float directionAngle2 = 0;
    private float directionAngle3 = 0;    

    protected void Awake(){
        LPPV_CarController = GetComponent<LPPV_CarController>();
        RoadGen = GetComponent<RoadGen>();

    }
    
    //setto la posizione iniziale della macchina e azzero la velocità

    public override void OnEpisodeBegin(){
        transform.localPosition = new Vector3(0f, 0f, 0f);
        Quaternion newRotation = Quaternion.Euler(0f, 0f, 0f);
        transform.localRotation = newRotation;
        counter = 0;

        foreach (var wheel in LPPV_CarController.wheels)
        {
            if (wheel.collider != null)
            {
                wheel.collider.motorTorque = 0f;
                wheel.collider.brakeTorque = Mathf.Infinity;
                wheel.collider.attachedRigidbody.angularVelocity = Vector3.zero;
            }
        }
        
        Rigidbody carRigidbody = LPPV_CarController.GetComponent<Rigidbody>();
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero; 
        
        RoadGen.RegenerateRoad();
    }

    //Observations: pozione del veicolo + Ray Perception Sensor 3D collegato all'oggetto
    
    public Material MN1;
    public Material MN2;
    public Material MN3;

    public override void CollectObservations(VectorSensor sensor) {
        //Rigidbody carRigidbody = LPPV_CarController.GetComponent<Rigidbody>();
        //sensor.AddObservation(carRigidbody.velocity);
        sensor.AddObservation(LPPV_CarController.CurrentSpeed);


    //////////////////////////////////////////////////////////////////////////////////////

        //Direzione 1o prossimo checkpoint 
        string parentNamePrefix = transform.parent != null ? transform.parent.name + "_" : ""; //per identificare a quale macchina appartiene il checkpoint

        string expectedCheck = parentNamePrefix + "Checkpoint_" + counter;  //per identificare il blocco di strada con il numero corretto
        Debug.Log("NOME : " + expectedCheck);
        GameObject checkpointdir = GameObject.Find(expectedCheck); //cerca il pezzo

        if (checkpointdir != null) //check se esiste
        {
            checkpointdir.gameObject.tag = "CheckpointN1";   //assegno tag 
            checkpointdir.gameObject.layer = LayerMask.NameToLayer("CheckpointN1"); //assegno layer
            checkpointdir.gameObject.GetComponent<MeshRenderer>().material = MN1; //assegno materiale
            directionAngle = Vector3.Dot(transform.forward.normalized, (checkpointdir.transform.position - transform.position).normalized); //calcolo angolo      
        }
        sensor.AddObservation(directionAngle); //passo angolo come osservazione
        Debug.Log(parentNamePrefix + "Angle" + directionAngle);

    //////////////////////////////////////////////////////////////////////////////////////

        //Direzione 2o prossimo checkpoint
        string expectedCheck2 = parentNamePrefix + "Checkpoint_" + (counter + 1);
        Debug.Log("NOME : " + expectedCheck2);
        GameObject checkpointdir2 = GameObject.Find(expectedCheck2);
        if (checkpointdir2 != null)
        {
            checkpointdir2.gameObject.tag = "CheckpointN2";   //assegno tag 
            checkpointdir2.gameObject.layer = LayerMask.NameToLayer("CheckpointN2"); //assegno layer
            checkpointdir2.gameObject.GetComponent<MeshRenderer>().material = MN2;
            directionAngle2 = Vector3.Dot(transform.forward.normalized, (checkpointdir2.transform.position - transform.position).normalized);            
        }
        sensor.AddObservation(directionAngle2);
        Debug.Log(parentNamePrefix + "Angle 2" + directionAngle2);

    //////////////////////////////////////////////////////////////////////////////////////

        //Direzione 3o prossimo checkpoint
        string expectedCheck3 = parentNamePrefix + "Checkpoint_" + (counter + 2);
        Debug.Log("NOME : " + expectedCheck3);
        GameObject checkpointdir3 = GameObject.Find(expectedCheck3);
        if (checkpointdir3 != null)
        {
            checkpointdir3.gameObject.tag = "CheckpointN3";   //assegno tag 
            checkpointdir3.gameObject.layer = LayerMask.NameToLayer("CheckpointN3"); //assegno layer
            checkpointdir3.gameObject.GetComponent<MeshRenderer>().material = MN3;
            directionAngle3 = Vector3.Dot(transform.forward.normalized, (checkpointdir3.transform.position - transform.position).normalized);            
        }
        sensor.AddObservation(directionAngle3);
        Debug.Log(parentNamePrefix + "Angle 3" + directionAngle3);
        //sensor.AddObservation(LPPV_CarController.CurrentSpeed);
    }

    

    //collisione checkpoint
    public Material MDeact;

    private void OnTriggerEnter(Collider other)
    {
        string parentNamePrefix = transform.parent != null ? transform.parent.name + "_" : "";

        string expectedName = parentNamePrefix + "Checkpoint_" + counter;
        Debug.Log("TEST" + expectedName);

        if (other.gameObject.tag == "CheckpointN1" && other.gameObject.name == expectedName)
        {
            AddReward(+1f);
            Debug.Log("Checkpoint.");
            other.gameObject.tag = "CheckpointDeact";
            other.gameObject.layer = LayerMask.NameToLayer("CheckpointD");
            other.gameObject.GetComponent<MeshRenderer>().material = MDeact;
            counter++; // Increment the counter
        }
    }
    
    //3 azioni principali + reward

    public override void OnActionReceived(ActionBuffers actions) {
        //SetReward(0f);
        float ActAcceleration = 0f;
        float ActSteer = 0f;
        bool ActBrake = false;

        switch (actions.DiscreteActions[0]){
            case 0 : ActAcceleration = 0f; break;
            case 1 : ActAcceleration = +1f; break;
            case 2 : ActAcceleration = -1f; break;
        }
        Debug.Log("Selected Acceleration: " + ActAcceleration);

        switch (actions.DiscreteActions[1]){
            case 0 : ActSteer = 0f; break;
            case 1 : ActSteer = +1f; break;
            case 2 : ActSteer = -1f; break;
        }
        Debug.Log("Selected Steer: " + ActSteer);

        switch (actions.DiscreteActions[2]){
            case 0 : ActBrake = false; break;
            case 1 : ActBrake = true; break;
        }
        Debug.Log("Selected Brake: " + ActBrake);

        LPPV_CarController.Move(ActAcceleration, ActSteer, ActBrake);
        Debug.Log("Current Speed: " + LPPV_CarController.CurrentSpeed);
        Debug.Log("Reward: " + GetCumulativeReward());
        
        //REWARD 1 : Negativo, se 2 o 3 ruote non toccano la strada l'episodio viene riniziato

        int notGroundedCount = 0; // Count of wheels that are not grounded

        for (int i = 0; i < LPPV_CarController.wheels.Length; ++i)
        {
            if (LPPV_CarController.wheels[i].collider != null && !LPPV_CarController.wheels[i].collider.isGrounded && transform.localPosition.y < 0.5f)
            {
                notGroundedCount++;
                Debug.Log("COUNT" + (notGroundedCount)); 
            }
        }

        if (notGroundedCount >= 2)
        {
            Debug.Log("VAL" + transform.position.y); 
            AddReward(-10f);
            EndEpisode();
        }

        //REWARD 2 : Negativo o Positivo, in base alla velocità del veicolo

        AddReward((float)(LPPV_CarController.CurrentSpeed / LPPV_CarController.topSpeed - 0.35)/10);
        Debug.Log("REWARD VELOCITA' " + ((float)(LPPV_CarController.CurrentSpeed / LPPV_CarController.topSpeed - 0.35)/10));

        //REWARD 3 : PIU' VICINO AL TRAGUARDO (ipotetico 200z)
        Vector3 traguardo = new Vector3(0.0f, 0.0f, 150.0f);
        Debug.Log("REWARD PROSSIMITA' " + (-(150 - transform.localPosition.z)/5000)); 
    
        AddReward(-(150 - transform.localPosition.z)/5000);

        //REWARD 4 : Positivo, al traguardo viene ricompensato e viene riniziato l'episodio

        if (transform.localPosition.z > 200f)
        {
            AddReward(+10f);
            EndEpisode(); 
        }
        
    }
    

    //HEURISTIC per il controllo manuale

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int HeuAcceleration = 0;
        if (Input.GetKey(KeyCode.W)) HeuAcceleration = 1;
        if (Input.GetKey(KeyCode.S)) HeuAcceleration = 2;

        int HeuSteer = 0;
        if (Input.GetKey(KeyCode.D)) HeuSteer = 1;
        if (Input.GetKey(KeyCode.A)) HeuSteer = 2;

        int HeuBrake = 0;
        if (Input.GetButton("Jump")) HeuBrake = 1;
        else HeuBrake = 0;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        if(discreteActions[0]!=HeuAcceleration){
            discreteActions[0] = HeuAcceleration;
        }
        if(discreteActions[1] !=HeuSteer){
            discreteActions[1] = HeuSteer;
        }
        if(discreteActions[2] != HeuBrake){
            discreteActions[2] = HeuBrake;
        }
        
        //discreteActions[0] = HeuAcceleration;
        //discreteActions[1] = HeuSteer;
        //discreteActions[2] = HeuBrake;
    }

}
