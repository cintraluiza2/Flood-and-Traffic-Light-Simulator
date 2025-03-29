using UnityEngine;
using UnityEngine.AI; // Para acessar o NavMeshAgent

namespace HealthbarGames
{
    public class HaloTrafficLight : TrafficLightBase
    {
        public Renderer RedRenderer;
        public GameObject RedHalo;

        public Renderer YellowRenderer;
        public GameObject YellowHalo;

        public Renderer GreenRenderer;
        public GameObject GreenHalo;

        public Material LightsOnMat;
        public Material LightsOffMat;

        public BoxCollider trafficLightCollider; // Referência ao BoxCollider próximo ao semáforo

        private bool mInitialized = false;
        private bool isRedLight = false; // Flag to check if the light is red
        private NavMeshAgent carAgent; // Reference to the NavMeshAgent
        private bool carInCollider = false; // Flag to check if the car is in the collider

        void Awake()
        {
            if (    (RedRenderer != null || RedHalo != null)
                &&  (YellowRenderer != null || YellowHalo != null)
                &&  (GreenRenderer != null || GreenHalo != null)
                &&  (trafficLightCollider != null) // Certifique-se de que o collider está atribuído
                )
            {
                mInitialized = true;
            }
            else
            {
                mInitialized = false;
                Debug.LogError("Some variables haven't been assigned correctly for HaloTrafficLight script.", this);
            }

            // Get the NavMeshAgent attached to the car
            carAgent = FindObjectOfType<NavMeshAgent>();
        }

        void OnTriggerEnter(Collider other)
        {
            // Verifica se o carro entrou no collider
            if (other.CompareTag("Car")) // Supondo que o carro tenha a tag "Car"
            {
                carInCollider = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Verifica se o carro saiu do collider
            if (other.CompareTag("Car"))
            {
                carInCollider = false;
            }
        }

        // Callback from TrafficLight - called when lights state has changed
        public override void OnLightStateChanged(bool redLightState, bool yellowLightState, bool greenLightState)
        {
            if (!mInitialized)
                return;

            isRedLight = redLightState; // Update the red light state

            // Set halos and materials based on the light states
            if (RedHalo != null)
                RedHalo.SetActive(redLightState);

            if (RedRenderer != null)
                RedRenderer.material = (redLightState) ? LightsOnMat : LightsOffMat;

            if (YellowHalo != null)
                YellowHalo.SetActive(yellowLightState);

            if (YellowRenderer != null)
                YellowRenderer.material = (yellowLightState) ? LightsOnMat : LightsOffMat;

            if (GreenHalo != null)
                GreenHalo.SetActive(greenLightState);

            if (GreenRenderer != null)
                GreenRenderer.material = (greenLightState) ? LightsOnMat : LightsOffMat;

            // Control car movement based on the signal
            if (carAgent != null)
            {
                if (isRedLight && carInCollider)
                {
                    carAgent.isStopped = true; // Stop the car if the light is red and the car is in the collider
                }
                else
                {
                    carAgent.isStopped = false; // Resume the car if the light is green or the car is out of the collider
                }
            }
        }
    }
}
