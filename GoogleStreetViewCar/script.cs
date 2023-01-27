using UnityEngine;

namespace Mod
{
    public class Mod
    {
        public static void Main()
        {
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Metal Cube"),
                    NameOverride = "Street View Car",
                    DescriptionOverride = "A car that can view the street.",
                    CategoryOverride = ModAPI.FindCategory("Vehicles"),
                    ThumbnailOverride = ModAPI.LoadSprite("preview.png"),
                    AfterSpawn = (Instance) =>
                    {
                        //streetViewer BODY
                        Rigidbody2D rb = Instance.GetComponent<Rigidbody2D>();
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("carBodyCol.png");
                        GameObject.Destroy(Instance.GetComponent<BoxCollider2D>());
                        Instance.gameObject.FixColliders();
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("carBody.png");
                        Instance.GetComponent<SpriteRenderer>().sortingOrder=0;
                        float direction = Instance.transform.localScale.x;
                        rb.mass = 1000f;

                        //WHEELS
                        GameObject w = ModAPI.FindSpawnable("Wheel").Prefab;

                        Vector2[] wps = { new Vector2(-2.42f, -1.6f), new Vector2(1.62f, -1.6f) };

                        StreetViewCar streetViewer = Instance.AddComponent<StreetViewCar>();
                        CarBehaviour car = Instance.AddComponent<CarBehaviour>();
                        car.WheelJoints = new WheelJoint2D[2];

                        streetViewer.objects = new GameObject[2];
                        streetViewer.wheels = new WheelJoint2D[2];

                        for (int i = 0; i < 2; i++)
                        {
                            GameObject wheel = GameObject.Instantiate(w, Instance.transform.position + new Vector3(wps[i].x * direction, wps[i].y, 0f), Quaternion.identity);
                            wheel.transform.localScale *= 1.15f;
                            wheel.GetComponent<Rigidbody2D>().mass = 50f;
                            wheel.GetComponent<SpriteRenderer>().sortingOrder = 1;
                            WheelJoint2D wj = Instance.AddComponent<WheelJoint2D>();
                            wj.connectedBody = wheel.GetComponent<Rigidbody2D>();
                            wj.anchor = wps[i];
                            wj.autoConfigureConnectedAnchor = true;
                            JointSuspension2D js = wj.suspension;
                            js.dampingRatio = 0.75f;
                            js.frequency = 5f;
                            wj.suspension = js;
                            wj.breakForce = 15000f;
                            car.WheelJoints[i] = wj;
                            streetViewer.wheels[i] = wj;
                            streetViewer.objects[i]=wheel;
                        }

                        //CAR BEHAVIOUR
                        car.MotorSpeed = -3000f;
                        car.Activated = false;
                        car.Phys = Instance.GetComponent<PhysicalBehaviour>();
                        car.IsBrakeEngaged = true;

                        streetViewer.car = car;
                        streetViewer.source=Instance.AddComponent<AudioSource>();
                        streetViewer.loop=ModAPI.LoadSound("loop.wav");
                        streetViewer.start=ModAPI.LoadSound("start.mp3");
                        streetViewer.stop=ModAPI.LoadSound("stop.mp3");
                        streetViewer.source.loop=true;
                        streetViewer.source.volume=0.75f;
                        streetViewer.source.minDistance=0.1f;
                        streetViewer.source.maxDistance=1f;
                    }
                }
            );
        }
    }

    public class StreetViewCar : MonoBehaviour
    {
        public GameObject[] objects;

        public WheelJoint2D[] wheels;

        public CarBehaviour car;

        public AudioSource source;
        public AudioClip loop;
        public AudioClip start;
        public AudioClip stop;

        bool looping = false;
        bool stopped = false;
        bool started = false;

        float speed = 20f;

        void Update()
        {
            if (car != null && car.Activated)
            {
                if(!started)
                {
                    source.clip = start;
                    source.Play();
                    source.loop = false;
                    stopped = false;
                    started = true;
                }
                
                if (speed < 250f) { speed+=0.05f; }

                for(int i=0; i<wheels.Length; i++)//excluding the front wheel
                {
                    if(i==1){wheels[i].useMotor=false;}//prevent car from tipping over when breaking
                    JointMotor2D jm = wheels[i].motor;
                    jm.maxMotorTorque = speed;
                    wheels[i].motor = jm;
                }

                if (!looping && speed > 30f)//the speed > 50 is just a means of giving the stat sfx time to play before the loop starts.
                {
                    source.clip=loop;
                    source.Play();
                    source.loop=true;
                    looping = true;
                }
            }
            else if(!stopped)
            {
                source.clip=stop;
                source.Play();
                source.loop=false;
                looping = false;
                started=false;
                stopped=true;
                speed = 20f;
            }
        }

        void OnDestroy()
        {
            foreach (GameObject o in objects) { GameObject.Destroy(o); }
        }
    }
}