// Copyright (c) 2018 Janis Kirsteins.
// Licenced under the MIT licence.
// See the LICENSE file in the project root for more information.

namespace DemoWallet
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Contains information about an Ethereum block.
    /// In the case of our wallet, we are interested in the block-number
    /// (for synchronization purposes) and the timestamp (for displaying
    /// to the user).
    /// </summary>
    internal struct Block
    {
        /// <summary>
        /// The block number.
        /// </summary>
        internal BigInteger Number;

        /// <summary>
        /// The timestamp of the block (i.e. when it was mined).
        /// </summary>
        internal DateTimeOffset Timestamp;
    }
}