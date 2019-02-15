using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ASAPToolkit.Unity.Middleware;

namespace ASAPToolkit.Unity {

    public class ASAPToolkitManager : MonoBehaviour, IMiddlewareListener {

        public int worldUpdateFrequency = 15;

        private float nextWorldUpdate = 0.0f;
        private MiddlewareBase middleware;

        Dictionary<string, ASAPAgent> agents;
        Dictionary<string, AgentSpecRequest> agentRequests;
        Dictionary<string, VJoint> worldObjects;

        void Awake() {
            agents = new Dictionary<string, ASAPAgent>();
            agentRequests = new Dictionary<string, AgentSpecRequest>();
            worldObjects = new Dictionary<string, VJoint>();
            middleware = GetComponent<MiddlewareBase>();
            if (middleware != null) middleware.Register(this);
        }

        void Update() {
            if (Time.time > nextWorldUpdate) {
                UpdateWorld();
            }
        }

        void UpdateWorld() {
            nextWorldUpdate = Time.time + 1.0f / worldUpdateFrequency;
            //if (worldObjects.Count == 0) return;

            ObjectUpdate[] objectUpdates = new ObjectUpdate[worldObjects.Count];
            int objectIdx = 0;
            foreach (KeyValuePair<string, VJoint> kvp in worldObjects) {
                objectUpdates[objectIdx] = new ObjectUpdate {
                    objectId = kvp.Key,
                    transform = kvp.Value.GetTransformArray()
                };
                objectIdx++;
            }

            middleware.Send(JsonUtility.ToJson(new WorldObjectUpdate {
                msgType = AUPROT.MSGTYPE_WORLDOBJECTUPDATE,
                nObjects = worldObjects.Count,
                objects = objectUpdates
            }));
        }

        void LateUpdate() {
            // TODO: Maybe it is preferable behavior only set new agent states once?
            foreach (string id in agents.Keys) {
                if (agents[id].agentState != null) {
                    agents[id].ApplyAgentState();
                }
            }
        }

        // Maybe parsing/etc. could be done in the communication thread better?
        public void OnMessage(string rawMsg) {
            AsapMessage asapMessage;
            try {
                asapMessage = JsonUtility.FromJson<AsapMessage>(rawMsg);
            } catch (System.Exception e) {
                Debug.LogWarning("Failed to parse incomming msg to JSON: " + rawMsg + "\n\n" + e);
                return;
            }

            switch (asapMessage.msgType) {
                case AUPROT.MSGTYPE_AGENTSPECREQUEST: // AgentSpecRequest type msg comming from ASAP
                    AgentSpecRequest agentSpecRequest = JsonUtility.FromJson<AgentSpecRequest>(rawMsg);
                    if (!agentRequests.ContainsKey(agentSpecRequest.agentId)) {
                        agentRequests.Add(agentSpecRequest.agentId, agentSpecRequest);
                        Debug.Log("Added agent request: " + agentSpecRequest.source + ":" + agentSpecRequest.agentId);
                        nextWorldUpdate = Time.time + 3.0f; // Delay world updates while setting up new agent...
                    } else {
                        Debug.LogWarning("Already preparing agentSpec for ID " + agentSpecRequest.agentId);
                    }
                    break;
                case AUPROT.MSGTYPE_AGENTSTATE:
                    if (!agents.ContainsKey(asapMessage.agentId)) break;
                    if (agents.ContainsKey(asapMessage.agentId)) {
                        if (agents[asapMessage.agentId].agentState == null)
                            agents[asapMessage.agentId].agentState = new AgentState();
                        JsonUtility.FromJsonOverwrite(rawMsg, agents[asapMessage.agentId].agentState);

                        agents[asapMessage.agentId].agentState.boneRotationsParsed = new Quaternion[agents[asapMessage.agentId].agentState.nBones];
                        agents[asapMessage.agentId].agentState.boneTranslationsParsed = new Vector3[2]; // TODO: be explicit in protocol how many bones are sent

                        if (agents[asapMessage.agentId].agentState.binaryBoneValues.Length > 0) {
                            byte[] binaryMessage = System.Convert.FromBase64String(
                                agents[asapMessage.agentId].agentState.binaryBoneValues);
                            using (BinaryReader br = new BinaryReader(new MemoryStream(binaryMessage))) {
                                for (int b = 0; b < agents[asapMessage.agentId].agentState.nBones; b++) {
                                    if (b < 2) agents[asapMessage.agentId].agentState.boneTranslationsParsed[b] = (new BoneTranslation(br)).VectorConverted();
                                    agents[asapMessage.agentId].agentState.boneRotationsParsed[b] = (new BoneLocalRotation(br)).QuaternionConverted();
                                }
                            }
                        } else {
                            for (int b = 0; b < agents[asapMessage.agentId].agentState.boneTranslations.Length; b++) {
                                agents[asapMessage.agentId].agentState.boneTranslationsParsed[b] = agents[asapMessage.agentId].agentState.boneTranslations[b].VectorConverted();
                            }

                            for (int b = 0; b < agents[asapMessage.agentId].agentState.boneValues.Length; b++) {
                                agents[asapMessage.agentId].agentState.boneRotationsParsed[b] = agents[asapMessage.agentId].agentState.boneValues[b].QuaternionConverted();
                            }
                        }

                        if (agents[asapMessage.agentId].agentState.binaryFaceTargetValues.Length > 0) {
                            byte[] binaryMessage = System.Convert.FromBase64String(
                                agents[asapMessage.agentId].agentState.binaryFaceTargetValues);
                            agents[asapMessage.agentId].agentState.faceTargetValues =
                                new float[agents[asapMessage.agentId].agentState.nFaceTargets];
                            using (BinaryReader br = new BinaryReader(new MemoryStream(binaryMessage))) {
                                for (int f = 0; f < agents[asapMessage.agentId].agentState.nFaceTargets; f++) {
                                    agents[asapMessage.agentId].agentState.faceTargetValues[f] =
                                        br.ReadSingle();
                                }
                            }
                        }
                    } else {
                        Debug.LogWarning("Can't update state for unknown agent: " + asapMessage.agentId);
                    }
                    break;
                default:
                    break;
            }

            HandleRequests();
        }

        void HandleRequests() {
            foreach (KeyValuePair<string, AgentSpecRequest> kv in agentRequests) {

                if (!agents.ContainsKey(kv.Key)) {
                    Debug.Log("agentId unknown: " + kv.Key);
                    continue;
                }

                if (kv.Value.source == "/scene") {
                    middleware.Send(JsonUtility.ToJson(agents[kv.Key].agentSpec));
                    Debug.Log("Sent agent spec for id=" + kv.Key);
                } else {
                    Debug.LogWarning("Initializing agents only possible from /scene!");
                }
            }

            agentRequests.Clear();
        }

        public void OnAgentInitialized(ASAPAgent agent) {
            if (agents.ContainsKey(agent.agentSpec.agentId)) {
                Debug.LogWarning("Agent with id " + agent.agentSpec.agentId + " already known. Ignored.");
            } else {
                Debug.Log("Agent added: " + agent.agentSpec.agentId);
                agents.Add(agent.agentSpec.agentId, agent);
            }
        }

        public void OnWorldObjectInitialized(VJoint worldObject) {
            if (worldObjects.ContainsKey(worldObject.id)) {
                Debug.LogWarning("WorldObject with id " + worldObject.id + " already known. Ignored.");
            } else {
                worldObjects.Add(worldObject.id, worldObject);
            }
        }

    }

}