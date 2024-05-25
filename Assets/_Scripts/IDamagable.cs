
public interface IDamagable
{
    float Health { get; set; }

    /// <summary>
    /// Takes in a float as incoming Damage and return the amount of damage dealt
    /// </summary>
    /// <param name="incomingDamage">incoming Damage</param>
    /// <returns> amount of damage dealt</returns>
    float TakeDamage(float incomingDamage);

}
