﻿using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;
using instaPicsWebJob.Model;
using System.Net.Mail;

namespace instaPicsWebJob
{
    public class Functions
    {
        public static void ImageRequest([QueueTrigger("imgconvertqueue")] string guid,
           [Table("userimgtable")] CloudTable table,
           [Blob("imgblob")] CloudBlobContainer blobContainer,
           TextWriter logger)
        {
            IEnumerable<CloudBlockBlob> listblobs = blobContainer.ListBlobs().OfType<CloudBlockBlob>();

            foreach(CloudBlockBlob blockBlob in listblobs)
            {
                if(blockBlob.Name == guid)
                {
                    //chemin des images
                    string filePathOriginal = Path.Combine(Path.GetTempPath(), guid);
                    string filePathOriginalThumb = Path.Combine(Path.GetTempPath(), "thumb" + guid);
                    string filePathBN = Path.Combine(Path.GetTempPath(), "BN" + guid);
                    string filePathBNThumb = Path.Combine(Path.GetTempPath(), "BNThumb" + guid);

                    //téléchargement de l'image pour pouvoir la réutiliser
                    using (var fileStream = File.OpenWrite(filePathOriginal))
                    {
                        blockBlob.DownloadToStream(fileStream);
                    }


                    //traitement + sauvegarde dans des fichiers temporaires
                    Bitmap BNImg = ImgProcessing.SetGrayscale(filePathOriginal);
                    BNImg.Save(filePathBN);

                    Image originalThumb = ImgProcessing.GetThumbnail(filePathOriginal);
                    originalThumb.Save(filePathOriginalThumb);

                    Image BNThumb = ImgProcessing.GetThumbnail(filePathOriginalThumb);
                    BNThumb.Save(filePathBNThumb);

                    //enregistrement dans les blobs
                    CloudBlockBlob blobBnImg = blobContainer.GetBlockBlobReference("BN" + guid);
                    blockBlob.UploadFromFile(filePathBN, FileMode.Open);

                    logger.WriteLine("Enregistrement dans le blobcontainer du fichier BN"+ guid);

                    CloudBlockBlob blobThumbImg = blobContainer.GetBlockBlobReference("thumb" + guid);
                    blockBlob.UploadFromFile(filePathOriginalThumb, FileMode.Open);

                    logger.WriteLine("Enregistrement dans le blobcontainer du fichier thumb" + guid);

                    CloudBlockBlob blobBnThumbImg = blobContainer.GetBlockBlobReference("BNThumb" + guid);
                    blockBlob.UploadFromFile(filePathBNThumb, FileMode.Open);

                    logger.WriteLine("Enregistrement dans le blobcontainer du fichier BNThumb" + guid);


                    //recherche de l'enregistrement dans la table qui correspond à l'image + mettre à jour l'enregistrement
                    TableQuery<UserImageEntity> query = new TableQuery<UserImageEntity>();

                    foreach (UserImageEntity entity in table.ExecuteQuery(query))
                    {
                        if(entity.imgOriginal == guid)
                        {
                            entity.imgBN = "BN" + guid;
                            entity.imgBNThumb = "BNThumb" + guid;
                            entity.imgOriginalThumb = "thumb" + guid;

                            TableOperation updateOperation = TableOperation.Replace(entity);

                            table.Execute(updateOperation);

                            logger.WriteLine("mise à jour de l'enregistrement dans la table correspondant à l'image " + guid);
                            break;
                        }
                    }

                    break;
                }
            }
        }

        public static void HandleGenerationImagePoison([QueueTrigger("imgconvertqueue-poison")] string guid,
            TextWriter logger)
        {
            // Envoi d'un email à l'administrateur
            logger.WriteLine("Impossible de traiter l'image {0}", guid);
            Functions.sendEmail(guid, logger);
        }

        // envoie un email, certaines infos sont manquantes comme l'adrese du smtpCLient et ses identifiants
        //source http://stackoverflow.com/questions/449887/sending-e-mail-using-c-sharp
        private static void sendEmail(string guid, TextWriter logger)
        {
            try
            {

                SmtpClient mySmtpClient = new SmtpClient("my.smtp.exampleserver.net");

                // set smtp-client with basicAuthentication
                mySmtpClient.UseDefaultCredentials = false;
                System.Net.NetworkCredential basicAuthenticationInfo = new
                   System.Net.NetworkCredential("username", "password");
                mySmtpClient.Credentials = basicAuthenticationInfo;

                // add from,to mailaddresses
                MailAddress from = new MailAddress("instapics@free.fr", "InstaPics");
                MailAddress to = new MailAddress("test2@example.com", "TestToName");
                MailMessage myMail = new System.Net.Mail.MailMessage(from, to);

                // add ReplyTo
                MailAddress replyto = new MailAddress("instapics@free.fr");
                myMail.ReplyToList.Add(replyto);

                // set subject and encoding
                myMail.Subject = "Erreur de traitement";
                myMail.SubjectEncoding = System.Text.Encoding.UTF8;

                // set body-message and encoding
                myMail.Body = "Impossible de traiter l'image " + guid;
                myMail.BodyEncoding = System.Text.Encoding.UTF8;
                // text or html
                myMail.IsBodyHtml = true;

                mySmtpClient.Send(myMail);
            }
            catch (Exception ex)
            {
                logger.WriteLine("Impossible d'envoyer le mail par rapport à l'image suivante {0}", guid);
            }
        }
    }
}
