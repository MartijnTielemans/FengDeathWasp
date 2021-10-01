using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    // Next three methods are for detecting collisions
    // For the ground
    public bool BottomCollisions(Vector3 bottomOffset, Vector3 bottomBounds)
    {
        Collider[] overlappedGround = Physics.OverlapBox(transform.position + bottomOffset, bottomBounds, transform.rotation, groundLayer);
        return (overlappedGround.Length > 0);
    }

    // For left wall
    public bool LeftCollisions(Vector3 leftOffset, Vector3 leftBounds)
    {
        Vector3 boxPos = transform.position + (transform.forward * leftOffset.x) + Vector3.up * leftOffset.y;
        return (CheckWallCollision(boxPos, leftBounds).Length > 0);
    }

    // For right wall
    public bool RightCollisions(Vector3 rightOffset, Vector3 rightBounds)
    {
        Vector3 boxPos = transform.position + (transform.forward * rightOffset.x) + Vector3.up * rightOffset.y;
        return (CheckWallCollision(boxPos, rightBounds).Length > 0);
    }

    // Checks if wall is overlapped, takes a position and bounds
    public Collider[] CheckWallCollision(Vector3 position, Vector3 bounds)
    {
        Collider[] overlappedWall = Physics.OverlapBox(position, bounds, transform.rotation, wallLayer);
        return overlappedWall;
    }
}
