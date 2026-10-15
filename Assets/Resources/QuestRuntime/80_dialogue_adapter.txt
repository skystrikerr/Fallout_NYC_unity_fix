// Native presentation for original dialogue trees. Keep closures in the JS runtime.
G.native.memory={failed:{},passed:{},karma:{}};
G.native.value=(v,a,c)=>typeof v==='function'?v(a,c):v;
G.native.text=function(v){const c=G.native.conversation,a=c&&G.native.npcs[c.actorId];let t=G.native.value(v,a,c);if(Array.isArray(t))t=t[Math.floor(Math.random()*t.length)];return String(t||'').replace(/\{(player|name)\}/g,G.player.name);};
G.native.close=function(){const c=G.native.conversation;if(c){const tree=G.native.dialogues[c.id];if(tree.onEnd)tree.onEnd(G.native.npcs[c.actorId],c);G.emit('dialogueEnd',c.id,G.native.npcs[c.actorId]);}G.native.conversation=null;G.native.send('dialogueEnd',{});};
G.dialogue.end=G.native.close;
G.native.check=function(check,r){
 if(!check)return null;
 if(check.gender)return {hide:G.rpg.gender!==String(check.gender)[0]};
 if(check.perk)return {hide:!G.rpg.hasPerk(check.perk),ok:true,label:'['+check.perk.replace(/_/g,' ')+']'};
 if(check.karma){const k=G.rpg.karmaLabel().toLowerCase();return {hide:!k.includes(String(check.karma).toLowerCase()),ok:true};}
 if(check.item||check.caps){const id=check.item||'caps',n=check.count||check.caps||1,ok=G.inventory.count(id)>=n;return {hide:!ok&&check.hideIfMissing,ok,locked:!ok,label:'['+id.replace(/_/g,' ')+' '+n+']',consume:check.take||check.pay?{id,n}:null};}
 const have=check.level?G.rpg.level:check.special?G.rpg.special[String(check.special).toUpperCase()[0]]:G.rpg.skill(check.skill||'speech');
 const need=check.level||check.value||50,label='['+(check.special||check.skill||'Level')+' '+need+']';
 if(check.skill==='speech'||check.chance){const chance=Math.max(2,Math.min(100,have>=need?100:Math.round(have/need*100+(G.rpg.special.L-5)*2)));return {chance,label:'['+(check.skill||'Speech')+' '+chance+'%]'};}
 return {ok:have>=need,locked:have<need&&!r.failNext,label};
};
G.native.enter=function(nodeId,depth){const c=G.native.conversation;if(!c)return;if(!nodeId||nodeId==='__end'){G.native.close();return;}if((depth||0)>12)throw Error('Dialogue redirect loop '+c.id);
 const tree=G.native.dialogues[c.id],node=nodeId==='__fail'?c.failNode:tree.nodes[nodeId],a=G.native.npcs[c.actorId];if(!node)throw Error('Missing dialogue '+c.id+'/'+nodeId);c.node=nodeId;
 if(node.onEnter){const redirect=node.onEnter(a,c);if(c!==G.native.conversation)return;if(typeof redirect==='string'&&redirect!==nodeId){G.native.enter(redirect,(depth||0)+1);return;}}
 c.responses=G.native.value(node.responses,a,c)||[];const choices=[];
 for(let i=0;i<c.responses.length;i++){const r=c.responses[i],key=c.id+':'+nodeId+':'+i;if(r.cond&&!r.cond(a,c))continue;if(r.check&&((G.native.memory.failed[key]&&!r.retry)||(G.native.memory.passed[key]&&!r.repeatable)))continue;const check=G.native.check(r.check,r);if(check?.hide)continue;choices.push({index:i,text:(check?.label?check.label+' ':'')+G.native.text(r.text||'Goodbye.'),enabled:!check?.locked});}
 G.native.send('dialogue',{id:c.id,actorId:c.actorId,node:nodeId,speaker:node.speaker||tree.speaker||a?.name||'',text:G.native.text(node.text),choices});G.emit('dialogueNode',c.id,nodeId,a);
};
G.native.choose=function(index){const c=G.native.conversation;if(!c)return;const r=c.responses[index],a=G.native.npcs[c.actorId];if(!r||(r.cond&&!r.cond(a,c)))return;
 const ck=G.native.check(r.check,r);if(ck?.locked||ck?.hide)return;const key=c.id+':'+c.node+':'+index;if(r.check&&((G.native.memory.failed[key]&&!r.retry)||(G.native.memory.passed[key]&&!r.repeatable)))return;
 let pass=!ck||ck.chance!==undefined?(!ck||Math.random()*100<ck.chance):ck.ok!==false,next=r.next;
 if(ck?.consume&&!G.inventory.remove(ck.consume.id,ck.consume.n))return;
 if(r.check&&(r.check.skill||r.check.special||r.check.level)){G.native.memory[pass?'passed':'failed'][key]=1;G.emit('speechCheck',{success:pass,skill:r.check.skill,special:r.check.special,value:r.check.value||0,actor:a});if(!pass)next=r.failNext??'__fail';}
 if(r.karma&&(!G.native.memory.karma[key]||r.repeatKarma)&&(pass||r.karmaOnFail)){G.rpg.addKarma(r.karma,'Dialogue');G.native.memory.karma[key]=1;}
 if(pass&&r.action){const result=r.action(a,c);if(c!==G.native.conversation)return;if(typeof result==='string'&&next==null)next=result;}
 next=G.native.value(next,a,c);if(c!==G.native.conversation)return;
 if(pass&&r.barter){G.native.tradeView(c.actorId,false,next||c.node);return;}
 if(r.end||next==null){G.native.close();return;}
 if(next==='__fail')c.failNode={text:'No. You have not convinced me.',responses:[{text:'Let us talk about something else.',next:c.node},{text:'Goodbye.',end:true}]};G.native.enter(next);
};
