﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;

namespace FotosAServidor
{
    public partial class MainPage : ContentPage
    {
        private class ImagenClase
        {
            public string ImagenPath { get; set; }
            public string ImagenNombre { get; set; }
        };
        private List<ImagenClase> imagenes = new List<ImagenClase>();
        private int MiMedidaID;
        private string Servidor;

        public MainPage()
        {
            InitializeComponent();
            Ping MiPing = new Ping();
            if (MiPing.Send("192.168.1.200").Status == IPStatus.Success)
            {
                Servidor = "192.168.1.200";
            }
            else
            {
                Servidor = "213.98.73.215";
            }

            //Servidor = "213.98.73.215";
            Servidor = "10.0.65.200";

        }

        private async void ButCamara_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsTakePhotoSupported && !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Mensaje..", "Captura de foto no soportada", "ok");
                return;
            }
            else
            {

                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "medidas",
                    SaveToAlbum = true,
                    CompressionQuality = 75,
                    CustomPhotoSize = 50,
                    PhotoSize = PhotoSize.Medium,
                    MaxWidthHeight = 2000,
                    DefaultCamera = CameraDevice.Rear
                });
                if (file == null) return;

                Añadir_A_Lista(file.Path.ToString());
            }
        }
            private async void ButGaleria_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {

                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Small,
            });

            if (file == null) return;
            Añadir_A_Lista(file.Path.ToString());
        }

        private void ButEnviar_Clicked(object sender, EventArgs e)
        {
            int MiContador;
            for (MiContador = 0; MiContador < imagenes.Count; MiContador++)
            {
                //MandarImagenAlServidor(imagenes[MiContador].ImagenPath.ToString(),"medidas", imagenes[MiContador].ImagenNombre);
                MandarImagenAlServidor(imagenes[MiContador].ImagenPath.ToString(), "fotoscorreo", imagenes[MiContador].ImagenNombre + (MiContador + 1).ToString("00"));
            }
            DisplayAlert("Resultado...", string.Format("Enviadas {0} imagenes...", MiContador), "OK");
        }
        private void MandarImagenAlServidor(string MiPath, string MiCarpetaDestino, string MiNombreDestino)
        {
            System.Uri url = new System.Uri("http://" + Servidor + "/Reparto/Service1.svc/UploadImage/" + MiCarpetaDestino + "," + MiNombreDestino);

            string filePath = MiPath;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "application/octet-stream";
            request.Method = "POST";
            request.ContentType = "image/jpeg";
            using (Stream fileStream = File.OpenRead(filePath))
            using (Stream requestStream = request.GetRequestStream())
            {
                int bufferSize = 1002048;
                byte[] buffer = new byte[bufferSize];
                int byteCount = 0;
                while ((byteCount = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    requestStream.Write(buffer, 0, byteCount);
                }
            }
            string result;
            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            //Console.WriteLine(result);
        }
            private void Añadir_A_Lista(string MiPath)
            {
                string MiNombreImagen = 
                    + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00")
                    + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00")
                    + (imagenes.Count + 1).ToString("00");

                ImagenClase imagen = new ImagenClase();
                imagen.ImagenPath = MiPath;
                imagen.ImagenNombre = MiNombreImagen.ToString();
                imagenes.Add(imagen);

                MiLista.ItemsSource = null;
                MiLista.ItemsSource = imagenes;

            }

        }
    }
