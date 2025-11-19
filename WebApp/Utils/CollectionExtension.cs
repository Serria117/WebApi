namespace WebApp.Utils;

public static class CollectionExtension
{
    extension<T>(ICollection<T> collection)
    {
        /// <summary>
        /// Check if the collection is not empty.
        /// </summary>
        /// <returns></returns>
        public bool NotEmpty() => collection.Count > 0;
        
        /// <summary>
        /// Check if the collection is empty.
        /// </summary>
        /// <returns></returns>
        public bool Empty() => collection.Count == 0;
    }
}