using System;
using System.Threading;
using Mono.Nat;

namespace ElectronChat
{
    class Nat
    {
        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        private INatDevice device;
        private Mapping mapping;

        public void Search()
        {
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();
            while (true)
            {
                Thread.Sleep(500000);
                NatUtility.StopDiscovery();
                NatUtility.StartDiscovery();
            }
        }

        private async void DeviceFound(object sender, DeviceEventArgs e)
        {
            await locker.WaitAsync();
            try
            {
                device = e.Device;
                mapping = new Mapping(Protocol.Tcp, Program.settings.Port, Program.settings.Port, 0, "ElectronChat");
                await device.CreatePortMapAsync(mapping);
                Console.WriteLine("Succesfully added UPNP entry");
            }
            catch (Exception) {}
            locker.Release();
        }

        public void RemoveUPNP()
        {
            try
            {
                device.DeletePortMap(mapping);
                Console.WriteLine("Succesfully removed UPNP entry");
            }
            catch (Exception) {}
        }
    }
}