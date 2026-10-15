G.native.terminalStatus={};G.native.terminalSession=null;
G.native.terminalView=function(text){const s=G.native.terminalSession,d=G.native.terminals[s.id],status=G.native.terminalStatus[s.id]||(G.native.terminalStatus[s.id]={});let choices=[],body=text||d.header||'ROBCO INDUSTRIES TERMLINK';
 const lock=Number(d.lock)||0;
 if(lock&&!status.unlocked){
  if(d.password&&G.inventory.count(d.password)>0)choices.push({index:-2,text:'USE PASSWORD',enabled:true});
  if(status.lockedOut)body='TERMINAL LOCKED OUT';
  else if(G.rpg.skill('science')<lock)body='Science '+lock+' required. Current: '+G.rpg.skill('science');
  else {if(!s.puzzle)s.puzzle=G.native.makePuzzle(lock,G.util.hashStr(s.id)^Date.now());body=(text||'Select the password. Likeness counts letters in matching positions.')+'\nAttempts remaining: '+s.puzzle.attempts;s.puzzle.words.forEach((w,i)=>{if(!w.removed)choices.push({index:i,text:w.w,enabled:true});});}
 }else {const entries=s.path.reduce((arr,i)=>arr[i].entries,d.entries);entries.forEach((e,i)=>{if(!e.cond||e.cond())choices.push({index:i,text:G.native.text(e.title),enabled:true});});if(s.path.length)choices.push({index:-1,text:'BACK',enabled:true});}
 G.native.send('terminal',{id:s.id,speaker:d.title||'ROBCO TERMLINK',text:body,choices});
};
G.terminals.open=function(id){if(!G.native.terminals[id]){G.notify('Terminal unavailable: '+id);return;}G.native.terminalSession={id,path:[],puzzle:null};G.native.terminalView();};
G.native.terminalChoose=function(index){const s=G.native.terminalSession;if(!s)return;const d=G.native.terminals[s.id],status=G.native.terminalStatus[s.id]||(G.native.terminalStatus[s.id]={}),lock=Number(d.lock)||0;
 if(lock&&!status.unlocked){let ok=index===-2&&d.password&&G.inventory.count(d.password)>0;if(!ok){if(status.lockedOut||!s.puzzle||G.rpg.skill('science')<lock)return;const word=s.puzzle.words[index];if(!word||word.removed)return;word.removed=true;ok=word.w===s.puzzle.password;if(!ok){s.puzzle.attempts--;if(s.puzzle.attempts<=0)status.lockedOut=true;G.native.terminalView('ENTRY DENIED. Likeness: '+G.native.likeness(word.w,s.puzzle.password));return;}}
  status.unlocked=true;if(index!==-2){G.emit('terminalHacked',s.id,lock);G.rpg.addXP(Math.max(5,Math.round(lock/2)),'Terminal hacked');}if(d.onUnlock)d.onUnlock(s.id,index!==-2);G.native.terminalView('ACCESS GRANTED');return;
 }
 if(index===-1){s.path.pop();G.native.terminalView();return;}const entries=s.path.reduce((arr,i)=>arr[i].entries,d.entries),e=entries[index];if(!e||(e.cond&&!e.cond()))return;
 let ret;if(e.action)ret=e.action(e,s.id);if(e.entries)s.path.push(index);G.native.terminalView(typeof ret==='string'?ret:G.native.text(e.text));
};
G.terminals.isUnlocked=id=>!!G.native.terminalStatus[id]?.unlocked||!G.native.terminals[id]?.lock;
G.terminals.isLockedOut=id=>!!G.native.terminalStatus[id]?.lockedOut;
G.native.vendorState={};
G.native.vendor=function(id){const a=G.native.npcs[id];if(!a?.merchant)return null;let m=G.native.vendorState[id];if(!m)m=G.native.vendorState[id]={caps:a.merchant.caps||0,stock:a.merchant.stock.map(e=>[e[0],e[1]])};return m;};
G.native.tradeView=function(id,sell,resume){const a=G.native.npcs[id],m=G.native.vendor(id);if(!m){G.notify('This character has nothing to trade.');return;}G.native.tradeSession={id,sell:!!sell,resume};let choices=[];
 const list=sell?G.items.all().filter(d=>d.id!=='caps'&&d.type!=='quest'&&d.value>0&&G.inventory.count(d.id)>0).map(d=>[d.id,G.inventory.count(d.id)]):m.stock;
 for(const row of list){const d=G.items.get(row[0]);if(!d||row[1]<=0)continue;const price=sell?G.rpg.priceSell(d,a):G.rpg.priceBuy(d,a);choices.push({id:d.id,text:d.name+' ('+row[1]+') — '+price+' caps',enabled:sell?m.caps>=price:G.inventory.caps()>=price&&!(d.type==='weapon'&&G.inventory.count(d.id)>0)});}
 G.native.send('trade',{id,sell:!!sell,resume,speaker:a.name,text:(sell?'SELL':'BUY')+'\nYour caps: '+G.inventory.caps()+'\nVendor caps: '+m.caps,choices});
};
G.native.trade=function(item){const s=G.native.tradeSession;if(!s)return;const a=G.native.npcs[s.id],m=G.native.vendor(s.id),d=G.items.get(item);if(!d||d.type==='quest'||item==='caps'||d.value<=0)return;let row=m.stock.find(e=>e[0]===item);
 if(s.sell){const price=G.rpg.priceSell(d,a);if(m.caps<price||!G.inventory.remove(item,1))return;m.caps-=price;G.inventory.addCaps(price);if(!row)m.stock.push([item,1]);else row[1]++;}
 else {const price=G.rpg.priceBuy(d,a);if(!row||row[1]<1||(d.type==='weapon'&&G.inventory.count(item)>0)||!G.inventory.remove('caps',price))return;G.inventory.add(item,1);row[1]--;m.caps+=price;}
 G.native.tradeView(s.id,s.sell,s.resume);
};

G.lockpick={start:(level,callback)=>{G.native.lockCallback=callback;G.native.send('lockpick',{level});}};
G.native.useDoor=id=>{const d=G.world.doors.find(d=>d.def.id===id);if(d)d.onInteract();};
G.native.unlock=()=>{if(G.native.lockCallback){G.native.lockCallback();G.native.lockCallback=null;G.emit('lockpicked');}};
G.terminals.close=()=>{G.native.terminalSession=null;G.native.close();};
G.terminals.unlock=id=>{(G.native.terminalStatus[id]||(G.native.terminalStatus[id]={})).unlocked=true;};
const originalFlush=G.native.flush;
G.native.flush=function(){for(const d of G.world.doors){if(d._nativeLock!==d._locked){d._nativeLock=d._locked;G.native.send('doorLock',{id:d.def.id,locked:d._locked});}}return originalFlush();};

// Present source epilogues in Unity instead of the browser DOM fallback.
G.hud={};
G.on('ending',(slides,id)=>{G.setState('cutscene');G.native.send('ending',{id,slides});});
G.native.finishEnding=()=>{G.setState('play');G.native.send('dialogueEnd',{});};
