namespace EB.Net
{
	public enum WebSocketCloseStatus
	{
		NormalClosure = 1000,
		EndpointUnavailable = 1001,
		ProtocolError = 1002,
		InvalidMessageType = 1003,
		Empty = 0,
		InvalidPayloadData = 1007,
		PolicyViolation = 1008,
		MessageTooBig = 1004,
		MandatoryExtension = 1010,
		InternalServerError = 1011
	}
}
