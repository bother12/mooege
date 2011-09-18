﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using D3Sharp.Net;
using D3Sharp.Net.Packets;
using D3Sharp.Utils;

namespace D3Sharp.Core.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public uint ServiceID { get; private set; }
        public uint ServerHash { get; private set; }
        public uint ClientHash { get; private set; }

        public ServiceAttribute(uint serviceID, uint serverHash, uint clientHash)
        {
            this.ServiceID = serviceID;
            this.ServerHash = serverHash;
            this.ClientHash = clientHash;
        }

        public ServiceAttribute(uint serviceID, string serviceName, uint clientHash)
        {
            this.ServiceID = serviceID;
            this.ServerHash = Service.GetServiceHashFromName(serviceName);
            this.ClientHash = clientHash;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceMethodAttribute: Attribute
    {
        public byte MethodID { get; set; }

        public ServiceMethodAttribute(byte methodID)
        {
            this.MethodID = methodID;
        }
    }

    public class Service
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        public Dictionary<uint, MethodInfo> Methods = new Dictionary<uint, MethodInfo>();

        public Service()
        {
            this.LoadMethods();
        }

        private void LoadMethods()
        {
            foreach (var methodInfo in this.GetType().GetMethods())
            {
                var attribute = Attribute.GetCustomAttribute(methodInfo, typeof(ServiceMethodAttribute));
                if (attribute == null) continue;

                this.Methods.Add(((ServiceMethodAttribute)attribute).MethodID, methodInfo);
            }
        }

        public void CallMethod(uint methodID, IClient client, Packet packet)
        {
            if (!this.Methods.ContainsKey(methodID))
            {
                Console.WriteLine("Unknown method 0x{0:x2} called on {1} ", methodID, this.GetType());
                return;
            }

            var method = this.Methods[methodID];
            //Console.WriteLine("[Client]: {0}:{1}", method.ReflectedType.FullName, method.Name);
            method.Invoke(this, new object[] {client, packet});
        }

        public static uint GetServiceHashFromName(string name)
        {
            var bytes = Encoding.ASCII.GetBytes(name);
            return bytes.Aggregate(0x811C9DC5, (current, t) => 0x1000193*(t ^ current));
        }
    }
}
