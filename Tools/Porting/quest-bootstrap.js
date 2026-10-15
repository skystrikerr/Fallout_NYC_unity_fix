// Native Unity host adapter. The original quest files remain unmodified.
var window=globalThis;
var console={log:function(){},warn:function(){},error:function(){}};
var performance={now:()=>Date.now()};
var G={errors:[],modules:[],hooks:{},savers:{},state:'play',time:{elapsed:0,real:0,hour:10,day:1,frame:0},QS:{has:()=>false,get:()=>null},settings:{},SHOT:false};
G.reportError=(where,e)=>{G.errors.push(where+': '+(e.stack||e.message||e));};
G.isStub=n=>!G[n]||G[n].__stub;
G.registerModule=m=>G.modules.push(m);
G.events={on:(name,fn)=>{(G.hooks[name]||(G.hooks[name]=[])).push(fn);return ()=>{};},emit:(name,...args)=>{for(const fn of G.hooks[name]||[])try{fn(...args);}catch(e){G.reportError(name,e);}}};
G.on=G.events.on;G.emit=G.events.emit;
G.save={register:(name,adapter)=>G.savers[name]=adapter};
G.native={queue:[],npcs:{},dialogues:{},terminals:{},props:{},serial:0,conversation:null};
G.native.send=(type,data)=>G.native.queue.push({type,data});
G.notify=text=>G.native.send('notice',{text:String(text)});
G.sfx={play:()=>{}};G.music={setMood:()=>{}};
G.radio={addNews:text=>G.native.send('news',{text}),stations:()=>[]};
G.setState=s=>{G.state=s;};G.isPlaying=()=>G.state==='play';
G.native.call=(action,data)=>JSON.parse(host(action,JSON.stringify(data||{}))||'null');
G.inventory={count:id=>G.native.call('count',{id})||0,caps:()=>G.native.call('count',{id:'caps'})||0,
 add:(id,n)=>{G.native.call('give',{id,count:n||1});G.emit('itemAdded',id,n||1);},remove:(id,n)=>G.native.call('take',{id,count:n||1}),
 addCaps:n=>G.native.call('give',{id:'caps',count:n}),isEquipped:id=>G.native.call('equipped',{id})};
G.rpg={level:1,karma:0,addXP:(n,reason)=>G.native.call('xp',{count:n,reason}),addKarma:n=>{G.rpg.karma+=n;},skill:id=>G.native.call('skill',{id})||25,special:id=>G.native.call('special',{id})||5};
G.dialogue={register:(id,tree)=>G.native.dialogues[id]=tree,start:(id,actor)=>G.native.talk(id,actor?.id),isOpen:()=>!!G.native.conversation};
G.terminals={register:(id,tree)=>G.native.terminals[id]=tree,open:id=>G.native.send('terminal',{id,definition:G.native.terminals[id]})};
G.fadeTransition=fn=>fn();G.input={actionPressed:()=>false,consumeAction:()=>{}};
G.native.actor=function(def){
 if(G.native.npcs[def.id])return G.native.npcs[def.id];
 const a=Object.assign({},def);a.pos=new THREE.Vector3(def.pos.x,def.pos.y||0,def.pos.z);a.object3d=new THREE.Object3D();a.object3d.position.copy(a.pos);a.dead=false;a.questNpc=true;
 a.say=text=>G.native.send('bark',{id:a.id,name:a.name,text});a.hostile=()=>!!a._hostile;a.setHostile=value=>{a._hostile=value;G.native.send('hostile',{id:a.id,hostile:!!value});};
 a.kill=()=>{a.dead=true;G.emit('actorKilled',a,G.player);G.native.send('dead',{id:a.id});};a.teleport=(x,y,z)=>{a.pos.set(x,y,z);G.native.send('actorMove',{id:a.id,pos:a.pos});};
 G.native.npcs[a.id]=a;G.native.send('actor',def);return a;
};
G.actors={spawn:G.native.actor,get:id=>G.native.npcs[id],all:()=>Object.values(G.native.npcs),remove:a=>{a._removed=true;G.native.send('remove',{id:a.id});},nearest:(p,r,filter)=>Object.values(G.native.npcs).filter(a=>!a._removed&&a.pos.distanceTo(p)<r&&(!filter||filter(a))).sort((a,b)=>a.pos.distanceTo(p)-b.pos.distanceTo(p))[0]};
G.props={spawn:(type,opts)=>{const id=opts.id||'native_prop_'+(++G.native.serial);const object3d=new THREE.Object3D();object3d.position.set(opts.x||0,opts.y||0,opts.z||0);const handle={id,object3d,pos:object3d.position,remove:()=>G.native.send('remove',{id})};G.native.props[id]={type,opts,handle};G.native.send('prop',{id,kind:type,opts});return handle;}};
G.combat={setRelation:(a,b,relation)=>G.native.send('relation',{a,b,relation})};
G.native.text=t=>typeof t==='function'?String(t()):String(t||'');
G.native.talk=function(id,actorId){const tree=G.native.dialogues[id];if(!tree)return;const actor=G.native.npcs[actorId];G.native.conversation={id,actorId,node:null};if(tree.onStart)tree.onStart(actor);const start=typeof tree.start==='function'?tree.start(actor):tree.start||'start';G.native.enter(start);};
G.native.enter=function(nodeId){const c=G.native.conversation;if(!c)return;if(!nodeId){G.native.conversation=null;G.native.send('dialogueEnd',{});return;}const tree=G.native.dialogues[c.id],node=tree.nodes[nodeId];if(!node)throw Error('Missing dialogue '+c.id+'/'+nodeId);c.node=nodeId;if(node.onEnter)node.onEnter(G.native.npcs[c.actorId]);const choices=[];for(let i=0;i<(node.responses||[]).length;i++){const r=node.responses[i];if(r.cond&&!r.cond())continue;choices.push({index:i,text:G.native.text(r.text)});}G.native.send('dialogue',{id:c.id,node:nodeId,speaker:tree.speaker||G.native.npcs[c.actorId]?.name||'',text:G.native.text(node.text),choices});};
G.native.choose=function(index){const c=G.native.conversation;if(!c)return;const tree=G.native.dialogues[c.id],node=tree.nodes[c.node],r=node.responses[index];if(!r||(r.cond&&!r.cond()))return;if(r.action)r.action(G.native.npcs[c.actorId]);const next=typeof r.next==='function'?r.next():r.next;G.native.enter(next);};
G.native.flush=()=>{const q=G.native.queue;G.native.queue=[];return JSON.stringify({commands:q,errors:G.errors.splice(0)});};
