using System;
using System.Collections.Generic;
using System.IO;

namespace KMeans
{
    class KMeans
    { 
        static void Main(string[] args)
        {
            bool random = false;
            int numClusters = 2;
            int numColumns = 3;
            String input = File.ReadAllText(@"C:\Users\kobyl\Source\Repos\KMeansPlus\KMeansPlus\bin\Debug\Data.txt");
            int i = 0, j = 0;
            double[][] result = new double[100][];
            foreach (var row in input.Split('\n'))
            {
                j = 0;
                if (i < 100)
                {
                    result[i] = new double[numColumns];
                    foreach (var col in row.Trim().Split('\t'))
                    {
                        if (row != "")
                        {
                            //Console.Write(col);
                            result[i][j] = Convert.ToDouble(col);
                            j++;
                        }
                    }
                    i++;
                }
            }
            Console.WriteLine("\n\n");
            Console.WriteLine("Начальные данные :\n");
            Console.WriteLine("-------------------");
            ShowData(result, 1, true, true);
            Console.WriteLine("\nУстановленое к-во кластеров " + numClusters);
            Console.WriteLine("\nКластеризация ...");
            int[] clustering = Cluster(result, numClusters, 0);

            Console.WriteLine("Кластеризация окончена \n");

            Console.WriteLine("Финальное распределение по кластерам:\n");
            ShowVector(clustering, true);

            Console.WriteLine("Откластеризированные данные :\n");
            ShowClustered(result, clustering, numClusters, 1);

            Console.ReadLine();
           // xlApp.Quit();
            //xlWorkbook.Close();
        } 

        public static int[] Cluster(double[][] rawData, int numClusters, int seed)
        {
            double[][] data = rawData;
            bool changed = true; 
            bool success = true; 

            double[][] means = InitMeans(numClusters, data, seed); 
            
            int[] clustering = new int[data.Length];

            int maxCount = data.Length * 10; 
            int ct = 0;
            while (changed == true && success == true && ct < maxCount) 
            {
                changed = UpdateClustering(data, clustering, means); 
                success = UpdateMeans(data, clustering, means);
                if (ct == 0)
                {
                    ShowClustered(data, clustering, numClusters, 1);
                }
                ++ct; 
            }
            for(int i=0; i<numClusters; i++)
            {
                if(means[0].Length==2)
                    Console.WriteLine("Центр кластера "+ i + ": " + means[i][0] + " " + means[i][1]);
                else Console.WriteLine("Центр кластера " + i +": "+ means[i][0] + " " + means[i][1]  + " " + means[i][2]);
            }
            Console.WriteLine("Колличество итераций : " + ct);
            return clustering;
        }

        private static double[][] InitMeans(int numClusters, double[][] data, int seed)
        {

            double[][] means = MakeMatrix(numClusters, data[0].Length);
            List<int> used = new List<int>(); 
            Random rnd = new Random(seed);
            int idx = rnd.Next(0, data.Length); // [0, data.Length-1]
            Array.Copy(data[idx], means[0], data[idx].Length);
            used.Add(idx);

            for (int k = 1; k < numClusters; ++k)
            {
                double[] dSquared = new double[data.Length]; // to closest mean
                int newMean = -1; // index of data item to be a new mean
                for (int i = 0; i < data.Length; ++i) 
                {
                    if (used.Contains(i) == true) continue; 

                    double[] distances = new double[k]; 
                    for (int j = 0; j < k; ++j)
                        distances[j] = Distance(data[i], means[k]); 
                    
                    int m = MinIndex(distances);
                    dSquared[i] = distances[m] * distances[m];
                }

                double p = rnd.NextDouble();
                double sum = 0.0; 
                for (int i = 0; i < dSquared.Length; ++i)
                    sum += dSquared[i];
                double cumulative = 0.00; 

                int ii = 0; 
                int sanity = 0; 
                while (sanity < data.Length * 2) 
                {
                    cumulative += dSquared[ii] / sum;
                    if (cumulative >= p && used.Contains(ii) == false)
                    {
                        newMean = ii; 
                        used.Add(newMean);
                        break;
                    }
                    ++ii; 
                    if (ii >= dSquared.Length) ii = 0; 
                    ++sanity;
                }

                Array.Copy(data[newMean], means[k], data[newMean].Length);
            }
            
            return means;

        } 

        private static double[][] Normalized(double[][] rawData)
        {
            double[][] result = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (int j = 0; j < result[0].Length; ++j) 
            {
                double colSum = 0.00;
                for (int i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                double mean = colSum / result.Length;
                double sum = 0.00;
                for (int i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                double sd = sum / result.Length;
                for (int i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }

        private static double[][] MakeMatrix(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        private static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            int numClusters = means.Length;
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; 

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.00;

            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                for (int j = 0; j < data[i].Length; ++j)
                {
                    means[cluster][j] += data[i][j]; 
                    
                }
                    
            }

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                {
                    means[k][j] /= clusterCounts[k]; 
                    //wConsole.WriteLine(means[k][j]);
                }
            return true;
        }

        private static bool UpdateClustering(double[][] data, int[] clustering,
          double[][] means)
        {
            int numClusters = means.Length;
            //Console.WriteLine(numClusters);
            bool changed = false;

            int[] newClustering = new int[clustering.Length]; 
            Array.Copy(clustering, newClustering, clustering.Length);

            double[] distances = new double[numClusters]; 

            for (int i = 0; i < data.Length; ++i)
            {

                for (int k = 0; k < numClusters; ++k) {
                    distances[k] = Distance(data[i], means[k]); 
                    //Console.WriteLine(data[i][k]);
                }

                int newClusterID = MinIndex(distances); // find closest mean ID
                                                        
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID; 
                }
            }

            if (changed == false)
                return false; 

            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; 

            Array.Copy(newClustering, clustering, newClustering.Length);
            return true; 
        }

        private static double Distance(double[] tuple, double[] mean)
        {
            double sumSquaredDiffs = 0.00;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }

        private static int MinIndex(double[] distances)
        {
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }

        static void ShowData(double[][] data, int decimals,
          bool indices, bool newLine)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                if (indices) Console.Write(i.ToString().PadLeft(3) + " ");
                for (int j = 0; j < data[i].Length; ++j)
                {
                    if (data[i][j] >= 0.0) Console.Write(" ");
                    Console.Write(data[i][j].ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
            }
            if (newLine) Console.WriteLine("");
        } // ShowData

        static void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i] + " ");
            if (newLine) Console.WriteLine("\n");
        }

        static void ShowVector(double[] vector, int decimals, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i].ToString("F" + decimals) + " ");
            if (newLine) Console.WriteLine("\n");
        }

        static void ShowClustered(double[][] data, int[] clustering,
          int numClusters, int decimals)
        {
            for (int k = 0; k < numClusters; ++k)
            {
                Console.WriteLine("===================");
                for (int i = 0; i < data.Length; ++i)
                {
                    int clusterID = clustering[i];
                    if (clusterID != k) continue;
                   // Console.Write(i.ToString().PadLeft(3) + " ");
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] >= 0.00) Console.Write(" ");
                        Console.Write(data[i][j].ToString("F" + decimals) + " ");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("===================");
            } 
        } 

    } 

} 
