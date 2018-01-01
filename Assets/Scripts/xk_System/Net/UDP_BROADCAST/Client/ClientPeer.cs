using System;
using System.Collections;
using System.Collections.Generic;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class ClientPeer : UDPLikeTCPPeer
	{
		public override void Update (double elapsed)
		{
			base.Update (elapsed);
			switch (m_state) 
			{
			case NETSTATE.CONNECTING:
				{
					
				}
				break;
			case NETSTATE.CONNECTED:
				{
					
				}
				break;
			case NETSTATE.DISCONNECTING:
				{

				}
				break;
			case NETSTATE.DISCONNECTED:
				{

				}
				break;
			default:
				break;
			}
		}
	}
}
