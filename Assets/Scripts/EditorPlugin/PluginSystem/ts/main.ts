import { T3Time, params } from "./model.js";

// Trigger compilation
import { Param } from "./model.js";
export { __params_init } from "./model.js";

import "./t3/main.js";
export { __t3_bridge_init } from "./t3/main.js";

globalThis.LogType = Object.freeze({ Info: 0, Success: 1, Warn: 2, Error: 3 });
globalThis.T3Time = T3Time;
globalThis.params = params;
globalThis.Param = Param;
