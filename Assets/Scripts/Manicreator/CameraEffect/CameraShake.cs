#pragma warning disable IDE0044 // 読み取り専用修飾子を追加します

using System;
using UnityEngine;

namespace Manicreator.CameraEffect
{
    /// <summary>
    /// カメラを振動させるコンポーネント
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        /// <summary>
        /// オシレータ
        /// </summary>
        [Serializable]
        public class Oscillator
        {
            /// <summary>
            /// 位相
            /// </summary>
            [Range(0f, 1f)]
            public float phase = 0;

            /// <summary>
            /// 位相をランダムにするか(true: 乱数を使う, false: phaseの値を使う)
            /// </summary>
            public bool useRandomPhase = true;

            /// <summary>
            /// 周波数
            /// </summary>
            public float freqency = 6f;

            /// <summary>
            /// 振幅
            /// </summary>
            public float amplitude = 0.1f;

            /// <summary>
            /// 振幅の遷移
            /// </summary>
            public AnimationCurve amplitudeCurve = AnimationCurve.Linear(0, 1, 1, 0);

            /// <summary>
            /// 位相を初期化する
            /// </summary>
            public void InitializePhase()
            {
                if (useRandomPhase)
                {
                    phase = UnityEngine.Random.Range(0f, 1f);
                }
            }
            
            /// <summary>
            /// 特定の時間に対する値を取得する
            /// </summary>
            /// <param name="time">特定の時間[0, 1]</param>
            /// <returns>対応する値</returns>
            public float CalcCurrent(float time)
            {
                float period = freqency * 2f * Mathf.PI;
                float amp = amplitudeCurve.Evaluate(time) * amplitude;
                return Mathf.Sin((time + phase) * period) * amp;
            }
        }

        /// <summary>
        /// 振動する時間
        /// </summary>
        [SerializeField]
        private float duration = 0.4f;

        /// <summary>
        /// X座標を振動させるか(true: 振動させる, false: 振動させない)
        /// </summary>
        [SerializeField]
        private bool useTranslationX = true;

        /// <summary>
        /// X座標のオシレータ
        /// </summary>
        [SerializeField]
        private Oscillator translationX;

        /// <summary>
        /// Y座標を振動させるか(true: 振動させる, false: 振動させない)
        /// </summary>
        [SerializeField]
        private bool useTranslationY = true;

        /// <summary>
        /// Y座標のオシレータ
        /// </summary>
        [SerializeField]
        private Oscillator translationY;

        /// <summary>
        /// Z座標を振動させるか(true: 振動させる, false: 振動させない)
        /// </summary>
        [SerializeField]
        private bool useTranslationZ = true;

        /// <summary>
        /// Z座標のオシレータ
        /// </summary>
        [SerializeField]
        private Oscillator translationZ;

        /// <summary>
        /// X軸回転するか(true: 振動させる, false: 振動させない)
        /// </summary>
        [SerializeField]
        private bool useRotationPitch = false;

        /// <summary>
        /// X軸回転のオシレータ
        /// </summary>
        [SerializeField]
        private Oscillator rotationPitch;

        /// <summary>
        /// Y軸回転するか(true: 振動させる, false: 振動させない)
        /// </summary>
        [SerializeField]
        private bool useRotationYaw = false;

        /// <summary>
        /// Y軸回転のオシレータ
        /// </summary>
        [SerializeField]
        private Oscillator rotationYaw;

        /// <summary>
        /// Z軸回転するか(true: 振動させる, false: 振動させない)
        /// </summary>
        [SerializeField]
        private bool useRotationRoll = false;

        /// <summary>
        /// Z軸回転のオシレータ
        /// </summary>
        [SerializeField]
        private Oscillator rotationRoll;

        //  振動する対象のカメラ
        private Camera cam;

        //  前フレームの位置オフセット
        private Vector3 previousOffset = Vector3.zero;
        //  位置オフセット
        private Vector3 offset = Vector3.zero;

        //  前フレームの回転オフセット
        private Quaternion previousRotationOffset = Quaternion.Euler(Vector3.zero);
        //  回転オフセット
        private Quaternion rotationOffset = Quaternion.Euler(Vector3.zero);

        //  振動開始からの時間(sec)
        private float elapsedTime = 0;

        /// <summary>
        /// 今振動しているか(true: 振動中, false: 振動していない)
        /// </summary>
        public bool IsShaking { get; private set; } = false;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (IsShaking)
            {
                elapsedTime += Time.deltaTime;
            }
            if (elapsedTime > duration)
            {
                StopShake();
            }

            UpdateOffset();
            ApplyOffset();
        }

        /// <summary>
        /// 振動を始める
        /// </summary>
        public void StartShake()
        {
            StopShake();
            InitializePhase();

            IsShaking = true;
        }

        /// <summary>
        /// 各オシレータの位相を初期化する
        /// </summary>
        private void InitializePhase()
        {
            translationX.InitializePhase();
            translationY.InitializePhase();
            translationZ.InitializePhase();
            rotationPitch.InitializePhase();
            rotationYaw.InitializePhase();
            rotationRoll.InitializePhase();
        }

        /// <summary>
        /// 振動を止める
        /// </summary>
        public void StopShake()
        {
            elapsedTime = 0f;
            IsShaking = false;

            ResetOffset();
        }

        /// <summary>
        /// カメラを本来の位置、回転に戻す
        /// </summary>
        private void ResetOffset()
        {
            cam.transform.position -= offset;
            previousOffset = Vector3.zero;
            offset = Vector3.zero;

            cam.transform.localRotation *= Quaternion.Inverse(rotationOffset);
            previousRotationOffset = Quaternion.Euler(Vector3.zero);
            rotationOffset = Quaternion.Euler(Vector3.zero);
        }

        /// <summary>
        /// オフセットを更新する
        /// </summary>
        private void UpdateOffset()
        {
            previousOffset = offset;
            previousRotationOffset = rotationOffset;

            if (IsShaking)
            {
                float ratio = elapsedTime / duration;

                if (useTranslationX)
                {
                    offset.x = translationX.CalcCurrent(ratio);
                }
                if (useTranslationY)
                {
                    offset.y = translationY.CalcCurrent(ratio);
                }
                if (useTranslationZ)
                {
                    offset.z = translationZ.CalcCurrent(ratio);
                }

                float rotPitch = (useRotationPitch) ? (rotationPitch.CalcCurrent(ratio)) : (0f);
                float rotYaw = (useRotationYaw) ? (rotationYaw.CalcCurrent(ratio)) : (0f);
                float rotRoll = (useRotationRoll) ? (rotationRoll.CalcCurrent(ratio)) : (0f);

                rotationOffset = Quaternion.Euler(rotPitch, rotYaw, rotRoll);
            }
        }

        /// <summary>
        /// オフセットを適用する
        /// </summary>
        private void ApplyOffset()
        {
            cam.transform.localPosition -= previousOffset;
            cam.transform.localPosition += offset;

            cam.transform.localRotation *= Quaternion.Inverse(previousRotationOffset);
            cam.transform.localRotation *= rotationOffset;
        }
    }
}

#pragma warning restore IDE0044 // 読み取り専用修飾子を追加します

