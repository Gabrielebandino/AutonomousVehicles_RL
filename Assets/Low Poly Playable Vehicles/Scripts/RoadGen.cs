using UnityEngine;

public class RoadGen : MonoBehaviour
{
    public GameObject squareForward;
    public GameObject checkpointPrefab;
    public int numberOfSquares = 3;
    public float squareSize = 10f;

    private Transform roadParent;
    private Vector3 nextLocalPosition;
    private int lastDirection = 3;

    private void Start()
    {
        GenerateRoad(); // Initial generation of the road
    }

    private void GenerateRoad()
    {
        // Destroy the old road if it exists
        if (roadParent != null)
        {
            DestroyImmediate(roadParent.gameObject);
        }

        // Create a new road parent as a child of the parent of this GameObject
        roadParent = new GameObject("Road").transform;

        // If the GameObject has a parent, set the new road parent to be the sibling of this GameObject
        if (transform.parent != null)
        {
            roadParent.SetParent(transform.parent, false);
        }
        else
        {
            roadParent.SetParent(null);
            roadParent.position = transform.position; // Set the position to match this GameObject if there is no parent
        }
        
        nextLocalPosition = Vector3.zero; // Start with zero since it's local to the new parent
        nextLocalPosition.y -= 0.049f; // Adjust due to car dimensions

        // Generate squares and checkpoints
        for (int i = 0; i < numberOfSquares; i++)
        {
            GenerateSquare(i);
        }
    }

    private void GenerateSquare(int index)
    {
        GameObject square = Instantiate(squareForward, roadParent);
        square.transform.localPosition = nextLocalPosition;

        // Get the parent name to use as a prefix. If there is no parent, use an empty string.
        string parentNamePrefix = transform.parent != null ? transform.parent.name + "_" : "";

        square.name = parentNamePrefix + "Block_" + index;

        // Instantiate and set up the checkpoint
        Vector3 checkpointPosition = new Vector3(0, 0, 0);
        if(index!=0){
            checkpointPosition = square.transform.position;
        }else{
            checkpointPosition = square.transform.position + 5 * square.transform.forward;
        }
        
        GameObject checkpoint = Instantiate(checkpointPrefab, checkpointPosition, Quaternion.identity, roadParent);
        checkpoint.name = parentNamePrefix + "Checkpoint_" + index;

        // Determine the next road segment's position and direction
        if (index > 1)
        {
            DetermineNextPositionAndDirection();
        }
        else
        {
            nextLocalPosition += Vector3.forward * 2 * squareSize;
        }
    }


    private void DetermineNextPositionAndDirection()
    {
        Vector3 offset = Vector3.zero;
        int randomDirection;
        do
        {
            randomDirection = Random.Range(0, 3);
        } 
        while (randomDirection == lastDirection);

        switch (randomDirection)
        {
            case 0:
                offset = Vector3.forward * 2 * squareSize;
                lastDirection = 0;
                break;
            case 1:
                offset = Vector3.right * squareSize + Vector3.forward * squareSize;
                lastDirection = 2;
                break;
            case 2:
                offset = Vector3.left * squareSize + Vector3.forward * squareSize;
                lastDirection = 1;
                break;
        }

        nextLocalPosition += offset;
    }

    // Call this function to generate a new road
    public void RegenerateRoad()
    {
        GenerateRoad();
    }
}
