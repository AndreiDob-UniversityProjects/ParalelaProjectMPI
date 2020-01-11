using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MPI;

namespace MPI_ImageFilter
{
    class Program
    {
        public static void master(string oldPath, string newPath)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Loading image... ");
            Bitmap image = new Bitmap(oldPath);
            Console.WriteLine("Image loaded ");

            int n = Communicator.world.Size;
            int begin = 1;
            int end = 1;
            int length = image.Height / (n - 1);

            int height = image.Height;
            int width = image.Width;
            int blurringDegree = 3;


            //sending data to slaves
            Console.WriteLine("Sending data... ");
            for (int i = 1; i < n; i++)
            {
                begin = end;
                end = end + length;
                if (i == n - 1)
                {
                    end = image.Height - 1;
                }

                Communicator.world.Send(image, i, 0);
                Communicator.world.Send(begin, i, 0);
                Communicator.world.Send(end, i, 0);
                Communicator.world.Send(blurringDegree, i, 0);
            }
            Console.WriteLine("Data sent ");

            //receiving data from slaves
            Console.WriteLine("Waiting for responses... ");
            int[,,] results = new int[height, width, 3];
            for (int i = 1; i < n; i++)
            {
                int beginIndex = Communicator.world.Receive<int>(i, 0);
                int endIndex = Communicator.world.Receive<int>(i, 0);
                int[,,] partialResult = Communicator.world.Receive<int[,,]>(i, 0);

                Console.WriteLine("\t Slave "+ i +" finished");

                for (int heightIndex = beginIndex; heightIndex < endIndex; heightIndex++)
                {
                    for (int widthIndex = 0; widthIndex < width; widthIndex++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            results[heightIndex, widthIndex, k] = partialResult[heightIndex, widthIndex, k];
                        }
                    }
                }
            }
            Console.WriteLine("Responses obtained ");

            //creating new image and saving it
            Console.WriteLine("Saving new image... ");
            ImageUtils imageOperations = new ImageUtils(image);
            imageOperations.saveModifiedImage(results, oldPath, newPath);
            Console.WriteLine("Saved image " + newPath + " " + (DateTime.Now - start));
        }

        public static void slave()
        {
            Bitmap image = Communicator.world.Receive<Bitmap>(0, 0);
            int begin = Communicator.world.Receive<int>(0, 0);
            int end = Communicator.world.Receive<int>(0, 0);
            int blurringDegree = Communicator.world.Receive<int>(0, 0);

            ImageUtils imageOperations = new ImageUtils(image);
            int[,,] result = imageOperations.sobbelOperatorForSection(image, begin, end, blurringDegree);

            Communicator.world.Send(begin, 0, 0);
            Communicator.world.Send(end, 0, 0);
            Communicator.world.Send(result, 0, 0);
        }
    

    static void Main(string[] args)
        {

            using (new MPI.Environment(ref args))
            {
                if (Communicator.world.Rank == 0)
                {
                    //master process
                    master("C:\\Users\\Asus\\source\\repos\\ProiectMPI2\\ProiectMPI2\\image.jpg", "C:\\Users\\Asus\\source\\repos\\ProiectMPI2\\ProiectMPI2\\image_sobbel.jpg");
                }
                else
                {
                    //child process
                    slave();
                }
            }
        }
    }
}
