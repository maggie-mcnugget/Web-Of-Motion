using UnityEngine;

public class WebShooter : MonoBehaviour
{
    [Header("Shooting")]
    public Transform webOrigin;
    public GameObject projectilePrefab;
    public float launchSpeed = 18f;
    public float gravity = -4f;

    [Header("Trajectory")]
    public GameObject trajectoryDotPrefab;
    public int dotCount = 20;
    public float timeBetweenDots = 0.1f;

    private GameObject[] trajectoryDots;
    private bool isAiming = false;
    private Camera mainCamera;

    private GameObject activeProjectile;

    void Start()
    {
        mainCamera = Camera.main;

        //create trajectory dots
        trajectoryDots = new GameObject[dotCount];

        for (int i = 0; i < dotCount; i++)
        {
            trajectoryDots[i] = Instantiate(trajectoryDotPrefab);
            trajectoryDots[i].SetActive(false);
        }
    }

    void Update()
    {
            // check for mouse click
            if (Input.GetMouseButtonDown(0))
            {
            // if projectile exists, destroy it and stop aiming
            if (activeProjectile != null)
                {
                WebProjectile web =
                    activeProjectile.GetComponent<WebProjectile>();

                if (web != null)
                    web.ReleaseWeb();

                Destroy(activeProjectile);

                activeProjectile = null;

                    isAiming = false;
                    HideTrajectory();

                    return;
                }

                // otherwise start aimaing new shot
                isAiming = true;
            }

            if (isAiming)
            {
                Vector2 direction = GetAimDirection();
                UpdateTrajectory(direction);
            }

            // shoot when mouse button is released
            if (Input.GetMouseButtonUp(0) && isAiming)
            {
                Vector2 direction = GetAimDirection();

                Shoot(direction);

                isAiming = false;
                HideTrajectory();
            }

    }

    Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        //mouse screen position in world space
        mousePosition.z = -mainCamera.transform.position.z;

        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    Vector2 GetAimDirection()
    {
        Vector2 mousePosition = GetMouseWorldPosition();

        Vector2 direction = mousePosition - (Vector2)webOrigin.position;

        return direction.normalized;
    }

    void UpdateTrajectory(Vector2 direction)
    {
        // projectiles starting velocity
        Vector2 startingVelocity = direction * launchSpeed;

        for (int i = 0; i < dotCount; i++)
        {
            // time passed
            float t = (i + 1) * timeBetweenDots;

            //predict horizontal position
            float x = webOrigin.position.x + startingVelocity.x * t;

            // predict vertical position
            float y = webOrigin.position.y
                + startingVelocity.y * t
                + 0.5f * gravity * t * t;

            // position of the trajectory dot
            trajectoryDots[i].transform.position = new Vector3(x, y, 0);

            trajectoryDots[i].SetActive(true);
        }
    }

    void Shoot(Vector2 direction)
    {
        // projectile at web origin
        GameObject projectile = Instantiate(
            projectilePrefab,
            webOrigin.position,
            Quaternion.identity
        );

        activeProjectile = projectile;

        // projectile starting velocity and gravity
        WebProjectile projectileScript = projectile.GetComponent<WebProjectile>();

        projectileScript.Launch(direction * launchSpeed, gravity);
    }

    void HideTrajectory()
    {
        foreach (GameObject dot in trajectoryDots)
        {
            dot.SetActive(false);
        }
    }
}
