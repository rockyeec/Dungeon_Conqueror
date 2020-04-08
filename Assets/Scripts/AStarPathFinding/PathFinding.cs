using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public static PathFinding Instance;
    private void Awake()
    {
        Instance = this;
    }

    Queue<NPCInput> requestQueue = new Queue<NPCInput>();
    NPCInput currentRequest;
    bool isProcessing;
    int maxIterationsPerFrame = 25;
    

    public void RequestPath(NPCInput npc)
    {
        requestQueue.Enqueue(npc);
        npc.pathRequestOrderPlaced = true;
        PerformNextRequest();
    }

    void PerformNextRequest()
    {
        Debug.Log("Queue Count: " + requestQueue.Count);
        if (isProcessing) return;

        if (requestQueue.Count == 0) return;

        isProcessing = true;
        currentRequest = requestQueue.Dequeue();


        StartCoroutine
        (
            FindPath
            (
                currentRequest.transform.position,
                (currentRequest.TargetDestination == null)
                ? currentRequest.lastTargetPosition
                : currentRequest.TargetDestination.position
            )
        );
         

        Debug.Log(Time.deltaTime);
        Debug.Log("Queue Count: " + requestQueue.Count);
    }

    void FinishedRequest()
    {
        isProcessing = false;
        currentRequest.pathRequestOrderPlaced = false;
        PerformNextRequest();
    }


    bool NeighboursAreNotWalkable(ref Node node)
    {
        if (!node.isWalkable)
        {
            List<Node> temp = MyGrid.Instance.GetNeighbours(node);
            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i].isWalkable)
                {
                    node = temp[i];
                    return false;
                }
            }

            FinishedRequest();
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator FindPath(Vector3 seeker, Vector3 target)
    {
        MyGrid grid = MyGrid.Instance;

        Node start = grid.GetNode(grid.ClampCoordinate(seeker));
        Node end = grid.GetNode(grid.ClampCoordinate(target));
        if (!NeighboursAreNotWalkable(ref start) && !NeighboursAreNotWalkable(ref end))
        {
            Heap<Node> openList = new Heap<Node>(grid.MaxSize);
            List<Node> closeList = new List<Node>();

            openList.Add(start);

            int loopsPerFrameCounter = 0;
            while (openList.Count > 0)
            {
                if (++loopsPerFrameCounter > maxIterationsPerFrame)
                {
                    loopsPerFrameCounter = 0;
                    yield return null;
                }

                Node currentNode = openList.RemoveTop();
                closeList.Add(currentNode);

                if (currentNode == end)
                {
                    break;
                }

                foreach (Node node in grid.GetNeighbours(currentNode))
                {
                    if (++loopsPerFrameCounter > maxIterationsPerFrame)
                    {
                        loopsPerFrameCounter = 0;
                        yield return null;
                    }

                    if (closeList.Contains(node))
                        continue;


                    int newCost = currentNode.gCost + GetDistance(currentNode, node) + node.extraCost;
                    if (newCost >= node.gCost && openList.Contains(node))
                        continue;


                    node.gCost = newCost;
                    node.hCost = GetDistance(node, end);
                    node.parent = currentNode;

                    if (openList.Contains(node))
                        continue;


                    openList.Add(node);
                }
            }

            StartCoroutine(SimplifyPathBeforeAddingTo(start, end, currentRequest.path));       
        }
    }


    IEnumerator SimplifyPathBeforeAddingTo(Node start, Node end, Stack<Vector3> path)
    {
        Vector3 prevDir = Vector3.zero;
        int loopsPerFrameCounter = 0;
        while (end != start)
        {
            if (++loopsPerFrameCounter > maxIterationsPerFrame * 2)
            {
                loopsPerFrameCounter = 0;
                yield return null;
            }

            //============================
            //MyGrid.Instance.path.Add(end);
            //============================


            Vector3 endPos = MyGrid.Instance.GetWorldPosition(end);
            Vector3 endPosNext = MyGrid.Instance.GetWorldPosition(end.parent);
            Vector3 curDir = endPosNext - endPos;
            if (curDir != prevDir)
                path.Push(endPos);
            prevDir = curDir;
            end = end.parent;            
        }
        FinishedRequest();
    }


    int GetDistance(Node a, Node b)
    {
        int x = Mathf.Abs(a.gridX - b.gridX);
        int y = Mathf.Abs(a.gridZ - b.gridZ);
        return Mathf.Min(x, y) * 14 + Mathf.Abs(x - y) * 10;
    }
}
