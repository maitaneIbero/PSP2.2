using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AlquilerBicicletas
{
    class Program
    {
        static void Main(string[] args)
        {
            BlockingCollection<int> stockBicis = new BlockingCollection<int>(100);
            BlockingCollection<int> stockBiciSecundario = new BlockingCollection<int>(100);

            /*Tiene que haber 3 task:
             * 1. Productor: Empresa de Bicicletas
             * 2. Consumidor y productor (alquila y devuelve bicicletas): Zona Gros
             * 3. Consumidor y productor (alquila y devuelve bicicletas): Zona Amara
            */
            
            //Productor, empresa que compra bicicletas.

            Task compraBici = Task.Run(() =>
            {
                int bici = 0;
                bool maxAlmacen = false;
                while (!maxAlmacen)
                {
                    stockBicis.Add(bici);
                    Console.WriteLine("La empresa de bicis ha comprado la bicicleta{0} y la tiene en el almacén principal.", bici);
                    bici++;
                    //Thread.Sleep(100);
                    if (bici == 200)
                    {
                        maxAlmacen = true;
                    }
                }
                stockBicis.CompleteAdding();
                Console.WriteLine("Cierre de almacén. Nadie podrá depositar bicicletas en dicho almacén", bici);
            });

            Task ZonaGros = Task.Run(() =>
            {
                while (!stockBicis.IsCompleted)
                {
                    int bici = -1;

                    try
                    {
                        bici = stockBicis.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("Error en Gros: ha habido problemas para alquilar bici.");
                    }

                    if (bici != -1)
                    {
                        Console.WriteLine("Un usuario en la zona Gros ha alquilado la bicicleta{0}.", bici);
                    }

                    if (!stockBicis.IsAddingCompleted)
                    {
                        if (bici % 3 == 0)
                        {
                            stockBicis.Add(bici);
                            Console.WriteLine("Un usuario en la zona Gros ha devuelto la bicicleta{0}.", bici);
                        }
                    }
                    else
                    {
                        if (bici % 3 == 0)
                        {
                            Console.WriteLine("El almacén principal está completo, se despositarán las bicis en el secundario.");
                            stockBiciSecundario.Add(bici);
                            Console.WriteLine("Un usuario en la zona Gros ha devuelto la bicicleta{0} al segundo almacén.", bici);
                        }
                    }
                }
                Console.WriteLine("En zona Gros no hay más bicis en el almacén.");
            });

            Task ZonaAmara = Task.Run(() =>
            {
                int bici = -1;
                while (!stockBicis.IsCompleted)
                {
                    try
                    {
                        bici = stockBicis.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("Error en Amara: ha habido problemas para alquilar bici.");
                    }

                    if (bici != -1)
                    {
                        Console.WriteLine("Un usuario en la zona Amara ha alquilado la bicicleta{0}.", bici);
                    }

                    if (!stockBicis.IsAddingCompleted)
                    {
                        if (bici % 5 == 0)
                        {
                            stockBicis.Add(bici);
                            Console.WriteLine("Un usuario en la zona Amara ha devuelto la bicicleta{0}.", bici);
                        }
                    }
                   else
                    {
                        if (bici % 5 == 0)
                        {
                            Console.WriteLine("El almacén principal está completo, se despositarán las bicis en el secundario.");
                            stockBiciSecundario.Add(bici);
                            Console.WriteLine("Un usuario en la zona Amara ha devuelto la bicicleta{0} al segundo almacén.", bici);
                        }
                    }

                }
                Console.WriteLine("En zona Amara no hay más bicis en el almacén.");
            });

            compraBici.Wait();
            ZonaGros.Wait();
            ZonaAmara.Wait();

            Console.WriteLine("El stock sobrante es {0} bicis.", stockBiciSecundario.Count);
            Console.Read();
        }
    }
}
