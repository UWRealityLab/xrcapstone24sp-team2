using UnityEngine;

public class AuditoriumController : MonoBehaviour
{
    #region Constants

    private readonly Vector3 _seatCenterOffset = new(0f, 0.42f, -2.97f);

    private readonly Vector3 _mainRowsStartingPosition = new(0f, -0.501f, 8.7f);

    private readonly Vector3[] _mainRowsVectors =
    {
        Vector3.zero,
        new(0.260f, 0.261f, 1.304f),
        new(0.000f, 0.522f, 2.609f),
        new(0.260f, 0.783f, 3.913f),
        new(0.000f, 1.043f, 5.217f),
        new(0.260f, 1.304f, 6.522f),
        new(0.000f, 1.565f, 7.826f),
        new(0.260f, 1.826f, 9.130f),
        new(0.000f, 2.087f, 10.435f),
        new(0.260f, 2.348f, 11.739f),
        new(0.000f, 2.609f, 13.043f),
        new(0.260f, 2.870f, 14.348f),
        new(0.000f, 3.130f, 15.652f),
        new(0.260f, 3.391f, 16.957f),
        new(0.000f, 3.652f, 18.261f),
        new(0.260f, 3.913f, 19.565f),
        new(0.000f, 4.174f, 20.870f),
        new(0.260f, 4.435f, 22.174f),
        new(0.000f, 4.696f, 23.478f),
        new(0.260f, 4.957f, 24.783f),
        new(0.000f, 5.217f, 26.087f),
        new(0.260f, 5.478f, 27.391f),
        new(0.000f, 5.739f, 28.696f),
        new(0.260f, 6.000f, 30.000f)
    };

    private Vector3[] _closeRowsVectors =
    {
        Vector3.zero,
        new(0.882f, 0.000f, -0.260f),
        new(1.764f, 0.000f, 0.000f),
        new(2.645f, 0.000f, -0.260f),
        new(3.527f, 0.000f, 0.000f),
        new(4.409f, 0.000f, -0.260f),
        new(5.291f, 0.000f, 0.000f),
        new(6.172f, 0.000f, -0.260f),
        new(7.054f, 0.000f, 0.000f),
        new(7.936f, 0.000f, -0.260f)
    };

    private readonly Vector3[] _seatsArc =
    {
        new(-15.000f, 0.000f, 0.000f),
        new(-14.535f, 0.000f, 0.242f),
        new(-14.070f, 0.000f, 0.483f),
        new(-13.602f, 0.000f, 0.717f),
        new(-13.115f, 0.000f, 0.910f),
        new(-12.628f, 0.000f, 1.103f),
        new(-12.139f, 0.000f, 1.289f),
        new(-11.638f, 0.000f, 1.443f),
        new(-11.137f, 0.000f, 1.596f),
        new(-10.636f, 0.000f, 1.747f),
        new(-10.126f, 0.000f, 1.868f),
        new(-9.617f, 0.000f, 1.990f),
        new(-9.107f, 0.000f, 2.112f),
        new(-8.592f, 0.000f, 2.208f),
        new(-8.077f, 0.000f, 2.303f),
        new(-7.562f, 0.000f, 2.399f),
        new(-7.044f, 0.000f, 2.475f),
        new(-6.526f, 0.000f, 2.549f),
        new(-6.007f, 0.000f, 2.624f),
        new(-5.487f, 0.000f, 2.681f),
        new(-4.966f, 0.000f, 2.738f),
        new(-4.445f, 0.000f, 2.791f),
        new(-3.923f, 0.000f, 2.833f),
        new(-3.401f, 0.000f, 2.875f),
        new(-2.878f, 0.000f, 2.908f),
        new(-2.355f, 0.000f, 2.937f),
        new(-1.832f, 0.000f, 2.961f),
        new(-1.308f, 0.000f, 2.979f),
        new(-0.785f, 0.000f, 2.992f),
        new(-0.261f, 0.000f, 2.999f),
        new(0.263f, 0.000f, 2.999f),
        new(0.787f, 0.000f, 2.993f),
        new(1.310f, 0.000f, 2.981f),
        new(1.834f, 0.000f, 2.963f),
        new(2.357f, 0.000f, 2.941f),
        new(2.880f, 0.000f, 2.912f),
        new(3.403f, 0.000f, 2.880f),
        new(3.925f, 0.000f, 2.839f),
        new(4.447f, 0.000f, 2.797f),
        new(4.968f, 0.000f, 2.745f),
        new(5.489f, 0.000f, 2.688f),
        new(6.010f, 0.000f, 2.631f),
        new(6.528f, 0.000f, 2.558f),
        new(7.047f, 0.000f, 2.482f),
        new(7.565f, 0.000f, 2.407f),
        new(8.080f, 0.000f, 2.312f),
        new(8.595f, 0.000f, 2.215f),
        new(9.110f, 0.000f, 2.118f),
        new(9.619f, 0.000f, 1.997f),
        new(10.129f, 0.000f, 1.875f),
        new(10.638f, 0.000f, 1.752f),
        new(11.140f, 0.000f, 1.602f),
        new(11.640f, 0.000f, 1.447f),
        new(12.141f, 0.000f, 1.293f),
        new(12.630f, 0.000f, 1.106f),
        new(13.116f, 0.000f, 0.912f),
        new(13.603f, 0.000f, 0.719f),
        new(14.071f, 0.000f, 0.484f),
        new(14.535f, 0.000f, 0.242f),
        new(15.000f, 0.000f, 0.000f)
    };

    private Vector3[] _closeSeatsArc =
    {
        new(-4.000f, 0.000f, 0f),
        new(-3.494f, 0.000f, 0.189f),
        new(-2.967f, 0.000f, 0.312f),
        new(-2.433f, 0.000f, 0.393f),
        new(-1.894f, 0.000f, 0.445f),
        new(-1.355f, 0.000f, 0.477f),
        new(-0.814f, 0.000f, 0.494f),
        new(-0.273f, 0.000f, 0.500f),
        new(0.268f, 0.000f, 0.498f),
        new(0.809f, 0.000f, 0.486f),
        new(1.349f, 0.000f, 0.463f),
        new(1.889f, 0.000f, 0.424f),
        new(2.427f, 0.000f, 0.370f),
        new(2.962f, 0.000f, 0.294f),
        new(3.490f, 0.000f, 0.179f),
        new(4.000f, 0.000f, 0.000f)
    };

    #endregion

    #region Components

    [SerializeField]
    private GameObject audienceMemberPrefab;

    #endregion

    #region Properties

    [SerializeField]
    private float _audienceFillFactor = 1f;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        foreach (var mainRowPosition in _mainRowsVectors)
        foreach (var seatPosition in _seatsArc)
            if (Random.value <= _audienceFillFactor)
                Instantiate(
                    audienceMemberPrefab,
                    _mainRowsStartingPosition + mainRowPosition + seatPosition + _seatCenterOffset,
                    Quaternion.identity
                );
    }

    // Update is called once per frame
    private void Update() { }
}
