namespace MUNity.BlazorServer.Extensions
{
	public static class MoreLinqExtensions
	{
		public static int FindIndex<T>(this IEnumerable<T> values, Predicate<T> predicate)
		{
			int result = -1;
			for (int i=0;i<values.Count();i++)
			{
				if (predicate(values.ElementAt(i)))
				{
					result = i;
					break;
				}
			}
			return result;
		}
	}
}
