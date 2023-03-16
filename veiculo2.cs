using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Globalization; 

public class veiculo2 : MonoBehaviour
{

    public Rigidbody rigidbody;
    public float torque = 90f;
    public float friccao = 1f;
    public float freio;
    public float pesoCadeira = 140;

    [Range(0, 5)]
    public float direcaoRodas;

    private float comandoAPP;
    private float comandoAPPfrente;
    private float anguloAPP;
    private float distanciaAPP;
   
    public Thread mThread;

    public enum Tracao
    {
        Dianteira,
        Traseira,
        QuatroRodas
    };
    public Tracao tracao;

    public WheelCollider[] wc_rodas;
    public Transform[] rodas;

    public float velocidadeMaxima; 

    public Material materialFreio;

    private float vertical, horizontal;
    private float distanciaMaxima;
    public Vector3 anguloCadeira;

    Vector3 oldPos;
    float totalDistance = 0;

    // including audio 
    public AudioClip somCadeira;
    public AudioSource rertz;

    void Start()
    {
        oldPos = transform.position;
        
        rigidbody.mass = pesoCadeira;

        rertz.clip = somCadeira;

        print("Iniciando Thread");
        ThreadStart ts = new ThreadStart(Update1);
        mThread = new Thread(ts);
        mThread.Start();

    }


    void Update()
    {
        ControlarCarro();
    }

    void ControlarCarro()
    {

        Vector3 distanceVector = transform.position - oldPos;
        float distanceThisFrame = distanceVector.magnitude;
        totalDistance += distanceThisFrame;
        oldPos = transform.position;

        vertical = comandoAPPfrente; 
        horizontal = comandoAPP; 
        anguloCadeira = new Vector3(0, anguloAPP, 0);
        distanciaMaxima = distanciaAPP;

      
        for (int i = 0; i < wc_rodas.Length; i++)
        {
            if (i > 4)
            {
                
                transform.rotation = Quaternion.LerpUnclamped(transform.rotation, Quaternion.Euler(anguloCadeira), Time.deltaTime);
                rertz.pitch = vertical/6f + horizontal/10f;
            }

            if ((vertical == 0) && (horizontal == 0))  
            {
                 wc_rodas[i].brakeTorque = freio;
            }

            
            else
            {
                float fric = friccao - Mathf.Abs((friccao * vertical));
                wc_rodas[i].brakeTorque = fric;
            }


            if (i < 5)
            {

                if (rigidbody.velocity.z <= -velocidadeMaxima || rigidbody.velocity.z >= velocidadeMaxima ||  totalDistance >= distanciaMaxima)
                {
                    wc_rodas[i].motorTorque = 0;
                    gameObject.GetComponent<Rigidbody>().velocity *= 0.92f; 
                }
                else
                {

                    if (tracao == Tracao.Dianteira && i < 2) 
                    {
                        wc_rodas[i].motorTorque = (vertical * torque); 



                        rertz.pitch = vertical / 6f + horizontal / 10f;
                    }
                    else if (tracao == Tracao.Traseira && i >= 2) 
                    {
                        wc_rodas[i].motorTorque = (vertical * torque); 




                        rertz.pitch = vertical / 6f + horizontal / 10f;

                    }
                    else
                    {
                        wc_rodas[i].motorTorque = (vertical * torque); 



                        rertz.pitch = vertical / 6f + horizontal / 10f;

                    }
                }
            }

           
            Vector3 posicao;
            Quaternion rotacao;

            wc_rodas[i].GetWorldPose(out posicao, out rotacao);
            rodas[i].position = posicao;
            rodas[i].rotation = rotacao;
        }
    }

    void Update1()
    {

        TcpListener server = null;
        try
        {

            // Set the TcpListener on port 8052.
            Int32 port = 8052;
            IPAddress localAddr = IPAddress.Parse("192.168.15.136");

            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            // Enter the listening loop.
            while (true)
            {
                Thread.Sleep(10);

                Debug.Log("Esperando por uma conexão... ");

                // Perform a blocking call to accept requests.
                TcpClient client = server.AcceptTcpClient();
                if (client != null)
                {
                    Debug.Log("Conectado!");
                }
                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Debug.Log("RECEBIDO: " + data);

                    string[] comandos = data.Split(':');

                    foreach (var word in comandos)
                    {
                       Debug.Log(word);
                    }

                    string cmd = comandos[0];
                    string qnt = comandos[1];

                    if(cmd.IndexOf("1.0") > -1)
                    {
                        comandoAPPfrente = float.Parse(cmd, CultureInfo.InvariantCulture.NumberFormat); 
                        distanciaAPP = float.Parse(qnt, CultureInfo.InvariantCulture.NumberFormat);

                    }

                    else
                    {
                        comandoAPP = float.Parse(cmd, CultureInfo.InvariantCulture.NumberFormat);

                        if (comandoAPP > 0)
                        {
                            anguloAPP = float.Parse(qnt, CultureInfo.InvariantCulture.NumberFormat);
                        }

                        if (comandoAPP < 0)
                        {
                            anguloAPP = float.Parse(qnt, CultureInfo.InvariantCulture.NumberFormat) * -1;
                        }

                        else
                        {
                            comandoAPPfrente = float.Parse(qnt, CultureInfo.InvariantCulture.NumberFormat);
                        }

                    }

                }

                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException:" + e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }

    }


}

