/*
 * This file is part of VLCSHARP.
 *
 * VLCSHARP is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * VLCSHARP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with VLCSHARP.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * Copyright 2012 Caprica Software Limited.
 */
using System;

namespace Caprica.VlcSharp.Player.Embedded {
 
    /**
     * Specification for a full-screen strategy implementation.
     */
    public interface FullScreenStrategy {

        /**
         * Attempt to enter full-screen mode.
         */
        void EnterFullScreenMode();

        /**
         * Exit full-screen mode.
         */
        void ExitFullScreenMode();

        /**
         * Test whether or not full-screen mode is currently active.
         *
         * @return <code>true</code> if full-screen mode is active; otherwise <code>false</code>
         */
        bool IsFullScreenMode();
    }
}
