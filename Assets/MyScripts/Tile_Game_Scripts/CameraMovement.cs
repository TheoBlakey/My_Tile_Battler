using UnityEngine;

namespace Assets.MyScripts.Tile_Game_Scripts
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;

        private float speedScale = 4;

        private Vector3 dragOrigin;

        private float zoomSpeed = 5f;
        private float minFOV = 0.3f;
        private float maxFOV = 60.0f;

        private void Update()
        {
            PanCamera();

        }

        private void PanCamera()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                float newFOV = cam.orthographicSize - (scroll * zoomSpeed);
                newFOV = Mathf.Clamp(newFOV, minFOV, maxFOV);

                cam.orthographicSize = newFOV;
            }


            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = cam.ScreenToViewportPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 difference = dragOrigin - cam.ScreenToViewportPoint(Input.mousePosition);

                cam.transform.position += difference * speedScale * cam.orthographicSize;
                dragOrigin = cam.ScreenToViewportPoint(Input.mousePosition);
            }




        }

        //public void ZoomIn()
        //{
        //    float newSize = cam.orthographicSize + zoomStep;
        //    cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
        //}

        //public void ZoomOut()
        //{
        //    float newSize = cam.orthographicSize - zoomStep;
        //    cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
        //}
    }
}