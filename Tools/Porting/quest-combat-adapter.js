// Original simulation; Unity owns the rendered character models and player controller.
G.worldRunning=()=>G.state==='play';
G.__cmb.vats={active:false};G.vats.exit=()=>{G.__cmb.vats.active=false;};
G.__cmb.showBark=(a,text)=>G.native.send('bark',{id:a.id,name:a.name,text});G.__cmb.hideBark=()=>{};G.__cmb.updateBarks=()=>{};G.__cmb.clearBarks=()=>{};
G.inventory.equipped=slot=>null;G.inventory.weight=()=>0;G.inventory.equippedApparel=()=>[];
G.player.alive=true;G.player.hp=100;G.player.maxHp=100;G.player.ap=100;G.player.maxAp=100;G.player.rads=0;G.player.sneaking=false;G.player.velocity=new THREE.Vector3();
G.player.eyePos=out=>(out||new THREE.Vector3()).copy(G.player.pos).add(new THREE.Vector3(0,1.62,0));
G.player.damage=(amount,info)=>{const value=G.rpg.mitigate(amount,(info||{}).type,info||{});G.native.call('damage',{count:value*100/G.rpg.maxHp()});};
G.player.addRads=n=>G.native.call('rads',{count:n});
const nativeSpawn=G.actors.spawn,nativeRemove=G.actors.remove;
G.actors.spawn=function(def){const a=nativeSpawn(def);if(!a)return null;G.native.npcs[a.id]=a;G.native.send('actor',{id:a.id,type:a.type,name:a.name,gender:a.gender||def.gender,outfit:def.outfit,dialogue:a.dialogue,pos:{x:a.pos.x,y:a.pos.y,z:a.pos.z},yaw:a.yaw,essential:a.essential,weapon:a.weaponId,height:a.height,radius:a.radius});return a;};
G.actors.remove=function(a){if(!a)return;nativeRemove(a);delete G.native.npcs[a.id];G.native.send('remove',{id:a.id});};
G.actors.all=G.actors.list;
G.native.combatStates=()=>G.actors.list().filter(a=>a.inScene).map(a=>({id:a.id,pos:{x:a.pos.x,y:a.pos.y,z:a.pos.z},yaw:a.yaw,animation:a._anim||'idle',dead:a.dead,hostile:a.hostile(),hp:a.hp,maxHp:a.maxHp,visible:!a._farHidden&&!a.disintegrated&&!a.burrowed,dialogue:a.dialogue}));
G.native.hit=function(id,amount,weapon,part,dx,dy,dz){const a=G.actors.get(id);if(!a)return;const def=G.items.get(weapon),w=def?.weapon||{};a.damage(amount,{source:'player',type:w.damageType||'ballistic',part:part||'torso',weaponId:weapon,dir:new THREE.Vector3(dx,dy,dz),point:a.centerPos()});G.emit('weaponFired',def);};
G.native.interactActor=function(id){const a=G.actors.get(id);if(!a)return;if(a.dead)a.search();else a.talk();};
G.on('actorKilled',(a,killer)=>{if(killer==='player'||killer===G.player)G.rpg.addXP(a.xp||10,'Defeated '+a.name);});
// Render-only procedural fire texture used by the source simulation (Unity draws its own effects).
var document={createElement:()=>({width:64,height:64,getContext:()=>({createRadialGradient:()=>({addColorStop:()=>{}}),fillRect:()=>{},strokeRect:()=>{},fillText:()=>{},beginPath:()=>{},moveTo:()=>{},lineTo:()=>{},stroke:()=>{},save:()=>{},restore:()=>{},translate:()=>{},rotate:()=>{},measureText:t=>({width:String(t).length*10})})})};

Object.defineProperty(G.player,'eye',{get:()=>G.player.eyePos()});G.player.forward=new THREE.Vector3(0,0,-1);G.player.shake=(amount,duration)=>G.native.call('shake',{count:amount});

G.fx={tracer:(from,to)=>G.native.send('fx',{kind:'tracer',from,to}),laserBeam:(from,to)=>G.native.send('fx',{kind:'laser',from,to})};
G.sfx.play=(name,opts)=>{opts=opts||{};G.native.send('fx',{kind:'sound',name,pos:opts.pos,volume:opts.volume,pitch:opts.pitch});return null;};

G.native.projectileSerial=0;G.native.projectileStates=()=>G.combat.projectiles().map(p=>({id:p._nativeId||(p._nativeId=++G.native.projectileSerial),kind:p.kind,pos:p.pos,velocity:p.vel}));
G.fx.explosion=(pos,radius,style)=>G.native.send('fx',{kind:'explosion',pos,radius,style});
G.fx.blood=(pos,dir,size,color)=>G.native.send('fx',{kind:'blood',pos,dir,size,color});
