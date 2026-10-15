from pathlib import Path
import json
source=Path('G:/fallout-nyc-port/_js_source')
out=Path('outputs/UnityPort/QuestRuntime');out.mkdir(parents=True,exist_ok=True)
def save(name,text): (out/(name+'.txt')).write_text(text,encoding='utf8')
save('00_bootstrap',Path('work/unity-port/quest-bootstrap.js').read_text(encoding='utf8'))
save('01_math',Path('outputs/Fallout New York Simple/resources/app/game/vendor/three.min.js').read_text(encoding='utf8'))
engine=(source/'src/core/engine.js').read_text(encoding='utf8')
utility=engine[engine.index('  const U = (G.util'):engine.index('  const handlers = {};')]
world=engine[engine.index('  const CELL = 20;'):engine.index('  // ---- Location discovery')]
save('02_foundation',"G.scene=new THREE.Scene();G.camera=new THREE.PerspectiveCamera();G.player={name:'Wanderer',pos:new THREE.Vector3(-65,0,-708),height:1.7,radius:.35,teleport:(x,y,z,yaw)=>G.native.send('travel',{x,y,z,yaw})};G.raycastables=[];(()=>{const THREE=window.THREE;"+utility+world+'})();')
save('03_layout',(source/'src/core/layout.js').read_text(encoding='utf8'))
for i,file in enumerate(['rpg/00_items_core.js','rpg/10_items_aid.js','rpg/12_items_apparel.js','rpg/14_items_misc.js','rpg/20_loot_tables.js','weapons/00_defs.js','rpg/30_rpg_core.js','rpg/32_perks.js']):save('04_'+str(i)+'_'+Path(file).stem,(source/'src'/file).read_text(encoding='utf8'))
for file in sorted((source/'src/combat').glob('*.js')):
 if file.name!='50_vats.js':save('05_combat_'+file.stem,file.read_text(encoding='utf8'))
save('09_native_combat',Path('work/unity-port/quest-combat-adapter.js').read_text(encoding='utf8'))
for i,file in enumerate(sorted((source/'src/quests').glob('*.js'))):
 if 'audit' not in file.name and file.name!='22_vault_party.js':save('10_'+str(i).zfill(2)+'_'+file.stem,file.read_text(encoding='utf8'))
data=json.loads(Path('outputs/UnityPort/OpenWorld/world-details.json').read_text(encoding='utf8'))
(out/'worldlogic.bytes').write_text(json.dumps({k:data[k] for k in ['colliders','doors','anchors']},separators=(',',':')),encoding='utf8')
save('80_dialogue_adapter',Path('work/unity-port/quest-dialogue-adapter.js').read_text(encoding='utf8'))
terminal=(source/'src/ui_interact/30_terminal.js').read_text(encoding='utf8')
puzzle=terminal[terminal.index('  const WORDS_RAW'):terminal.index('  // DOM')];puzzle=puzzle[:puzzle.rfind('  // ------------------------------------------------------------------')]
save('81_terminal_puzzle',"(()=>{const X={};"+puzzle+"G.native.makePuzzle=makePuzzle;G.native.likeness=likeness;})();")
save('82_services',Path('work/unity-port/quest-services-adapter.js').read_text(encoding='utf8'))
save('90_adapters',r'''
G.native.initialize=function(json){const data=JSON.parse(json);G.landmarks={getAnchor:(site,name)=>data.anchors[site]?.[name]};G.map={_: {anchors:data.anchors},getAnchors:id=>data.anchors[id]||{},getAnchor:(site,name)=>data.anchors[site]?.[name]};
 for(const c of data.colliders)G.world.addCollider(c.center[0],c.center[2],c.size[0],c.size[2],c.center[1]-c.size[1]/2,c.center[1]+c.size[1]/2,c.yaw,{tag:c.tag,surface:c.surface});
 for(const d of data.doors)G.world.addDoor({id:d.id,pos:new THREE.Vector3(d.pos.x,d.pos.y,d.pos.z),label:d.name,target:d.target,locked:d.locked,keyId:d.keyId});
 G.items.spawnPickup=(id,count,pos,options)=>{const base='native_pickup_'+id+'_'+[pos.x,pos.y,pos.z].map(v=>Number(v||0).toFixed(3)).join('_');G.native.pickupIds=G.native.pickupIds||{};const key=base+'_'+(G.native.pickupIds[base]=(G.native.pickupIds[base]||0)+1);const handle={key,id,itemId:id,pos,object3d:new THREE.Object3D(),remove:()=>G.native.send('remove',{id:key})};G.native.props[key]={kind:'pickup',handle};G.native.send('pickup',{id:key,itemId:id,count,pos});return handle;};
 for(const mod of G.modules.slice().sort((a,b)=>a.order-b.order))if(mod.init)mod.init(G);G.emit('initDone');
 return JSON.stringify({quests:G.quests.list().length,dialogues:Object.keys(G.native.dialogues).length,terminals:Object.keys(G.native.terminals).length,items:G.items.all().length,errors:G.errors});
};
G.native.surface=function(saved){const state=saved?JSON.parse(saved):null;if(state?.rpg)G.rpg._deserialize(state.rpg);if(state?.dialogue)G.native.memory=state.dialogue;if(state?.terminals)G.native.terminalStatus=state.terminals;if(state?.vendors)G.native.vendorState=state.vendors;if(state?.combat)G.savers.combat.load(state.combat);if(state?.doors)G.world.restoreDoorLocks(state.doors);G.savers.quests.load(state?(state.quests||state):{q:{mq01:{s:'completed',st:'leave',log:[],t:0}},flags:{left_vault:true,got_pipboy:true},tracked:'mq02'});G.emit('loaded');const moe=G.native.dialogues.dlg_gc_moe;if(moe&&!moe._nativeStart){moe._nativeStart=moe.start;moe.start=()=>G.quests.isComplete('mq02')?'bar':moe._nativeStart();}if(!state){G.inventory.add('bobby_pin',5);G.inventory.add('caps',32);}if(!G.quests.isStarted('mq02'))G.quests.start('mq02');return G.native.flush();};
G.native.tick=function(x,y,z,dt){G.player.pos.set(x,y,z);G.time.elapsed+=dt;G.time.real+=dt;G.time.frame++;for(const mod of G.modules)if(mod.update)try{mod.update(dt,dt);}catch(e){G.reportError('module:'+mod.name,e);}G.native.send('actorStates',{states:G.native.combatStates()});G.native.send('projectileStates',{states:G.native.projectileStates()});return G.native.flush();};
G.native.save=()=>JSON.stringify({quests:G.savers.quests.save(),rpg:G.rpg._serialize(),dialogue:G.native.memory,terminals:G.native.terminalStatus,vendors:G.native.vendorState,combat:G.savers.combat.save(),doors:G.world.unlockedDoorKeys()});
G.native.journal=()=>JSON.stringify({quests:G.quests.list(),markers:G.quests.activeMarkers(true)});
''')
print('Prepared original quest runtime scripts',len(list(out.glob('*.txt'))))
