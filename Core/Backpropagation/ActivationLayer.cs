﻿/***********************************************************************************************
 COPYRIGHT 2008 Vijeth D

 This file is part of NeuronDotNet.
 (Project Website : http://neurondotnet.freehostia.com)

 NeuronDotNet is a free software. You can redistribute it and/or modify it under the terms of
 the GNU General Public License as published by the Free Software Foundation, either version 3
 of the License, or (at your option) any later version.

 NeuronDotNet is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with NeuronDotNet.
 If not, see <http://www.gnu.org/licenses/>.

***********************************************************************************************/

using System;
using System.Runtime.Serialization;

namespace NeuronDotNet.Core.Backpropagation
{
    /// <summary>
    /// 活动层是一层活动神经元。
    /// </summary>
    [Serializable]
    public abstract class ActivationLayer : Layer<ActivationNeuron>
    {
        internal bool useFixedBiasValues = false;

        /// <summary>
        /// 获取或设置表示是否使用固定神经元偏差值的布尔值
        /// </summary>
        /// <value>
        /// 一个布尔值，表示激活神经元的偏差值是否在训练时学习。
        /// </value>
        public bool UseFixedBiasValues
        {
            get { return useFixedBiasValues; }
            set { useFixedBiasValues = value; }
        }

        /// <summary>
        /// 构造激活层的实例
        /// </summary>
        /// <param name="neuronCount">
        /// 图层中的神经元数量
        /// </param>
        /// <exception cref="ArgumentException">
        /// 如果<c> neuronCount </ c>为零或负数
        /// </exception>
        protected ActivationLayer(int neuronCount)
            : base(neuronCount)
        {
            for (int i = 0; i < neuronCount; i++)
            {
                neurons[i] = new ActivationNeuron(this);
            }
        }

        /// <summary>
        /// 反序列化构造函数
        /// </summary>
        /// <param name="info">
        /// 序列化信息反序列化和获取数据
        /// </param>
        /// <param name="context">
        /// 要使用的序列化上下文
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 如果<c> info </ c>是<c> null </ c>
        /// </exception>
        public ActivationLayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.useFixedBiasValues = info.GetBoolean("useFixedBiasValues");

            double[] biasValues = (double[])info.GetValue("biasValues", typeof(double[]));
            for (int i = 0; i < biasValues.Length; i++)
            {
                neurons[i] = new ActivationNeuron(this);
                neurons[i].bias = biasValues[i];
            }
        }

        /// <summary>
        /// 使用序列化图层所需的数据填充序列化信息
        /// </summary>
        /// <param name="info">
        /// 用于填充数据的序列化信息
        /// </param>
        /// <param name="context">
        /// 要使用的序列化上下文
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 如果<c> info </ c>是<c> null </ c>
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("useFixedBiasValues", useFixedBiasValues);

            double[] biasValues = new double[neurons.Length];
            for (int i = 0; i < neurons.Length; i++)
            {
                biasValues[i] = neurons[i].bias;
            }

            info.AddValue("biasValues", biasValues, typeof(double[]));
        }

        /// <summary>
        /// 初始化所有神经元，使他们准备好接受新鲜训练。
        /// </summary>
        public override void Initialize()
        {
            if (initializer != null)
            {
                initializer.Initialize(this);
            }
        }

        /// <summary>
        /// 将神经元误差设置为实际输出和预期输出之间的差
        /// </summary>
        /// <param name="expectedOutput">
        /// 预期输出向量
        /// </param>
        /// <returns>
        /// 均方误差
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 如果<c> expectedOutput </ c>为<c> null </ c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 如果<c> expectedOutput </ c>的长度不同于神经元的数量
        /// </exception>
        public double SetErrors(double[] expectedOutput)
        {
            // 验证
            Helper.ValidateNotNull(expectedOutput, "expectedOutput");
            if (expectedOutput.Length != neurons.Length)
            {
                throw new ArgumentException("Length of ouput array should be same as neuron count", "expectedOutput");
            }

            // 设置错误，评估均方误差
            double meanSquaredError = 0d;
            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i].error = expectedOutput[i] - neurons[i].output;
                meanSquaredError += neurons[i].error * neurons[i].error;
            }
            return meanSquaredError;
        }

        /// <summary>
        /// 评估层中所有神经元的误差
        /// </summary>
        public void EvaluateErrors()
        {
            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i].EvaluateError();
            }
        }

        /// <summary>
        /// 该层中的所有神经元使用的激活函数
        /// </summary>
        /// <param name="input">
        /// 电流输入到神经元
        /// </param>
        /// <param name="previousOutput">
        /// 神经元上的先前输出
        /// </param>
        /// <returns>
        /// 激活的值
        /// </returns>
        public abstract double Activate(double input, double previousOutput);

        /// <summary>
        /// 该层中所有神经元使用的导数函数
        /// </summary>
        /// <param name="input">
        /// 电流输入到神经元
        /// </param>
        /// <param name="output">
        /// 电流输出（激活）在神经元
        /// </param>
        /// <returns>
        /// 激活函数的导数的结果
        /// </returns>
        public abstract double Derivative(double input, double output);
    }
}
