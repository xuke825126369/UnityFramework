using System.Collections;
using System.Collections.Generic;

namespace xk_System.Net.UDP.Client
{
	public class ClientPeer : SocketPeer
	{
		public override void Update ()
		{
			base.Update ();

			switch (m_state) {
			case NetState.connecting:
				{
					
				}
				break;
			case NetState.connected:
				{
					
				}
				break;
			case NetState.disconnecting:
				{

				}
				break;
			case NetState.disconnected:
				{

				}
				break;
			default:
				break;
			}
		}
	}
}
